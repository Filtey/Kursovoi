using ClosedXML.Excel;
using Google.Protobuf.WellKnownTypes;
using StoreSystem.ConnectToDB.Model;
using StoreSystem.ConnectToDB.Model.ApiCRUDs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media;


namespace StoreSystem.Classes
{
    public static class ImportExportTovarToXLSX
    {
        // ====== ПУБЛИЧНОЕ API ======

        
        public static void ExportToExcel(APIClass api, string filePath)
        {
            var sklads = api.SkladList() ?? new List<Sklad>();
            var tovars = api.TovarList() ?? new List<Tovar>();
            var tovarById = tovars.ToDictionary(t => t.Tovar_id);

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Склад");

            // ВНИМАНИЕ: включаем Tovar_Id — это ключ для точного обновления
            string[] headers = {
        "Tovar_Id","Артикул","Название","Тип","Производитель_Id",
        "Дата_производства","Гарантия_мес","Годен_до",
        "Ед_изм","Закуп_цена","Розничная_цена","Остаток","Комментарий"
    };
            for (int i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];

            int r = 2;
            foreach (var s in sklads)
            {
                if (!tovarById.TryGetValue(s.Tovar_id, out var t)) continue;

                ws.Cell(r, 1).Value = t.Tovar_id;
                ws.Cell(r, 2).Value = t.Artikul;
                ws.Cell(r, 3).Value = t.Name ?? "";
                ws.Cell(r, 4).Value = t.Type_tovar ?? "";
                ws.Cell(r, 5).Value = t.id_Manufacturer;

                // DateOnly? -> DateTime? (ClosedXML любит DateTime)
                DateTime? dtProd = t.Production_date == default ? null : t.Production_date;
                DateTime? dtUntil = t.Valid_until.HasValue ? t.Valid_until.Value : null;
                if (dtProd != null) ws.Cell(r, 6).Value = dtProd;
                if (dtUntil != null) ws.Cell(r, 8).Value = dtUntil;

                ws.Cell(r, 7).Value = t.Manufacturer_warranty;
                ws.Cell(r, 9).Value = s.unit ?? "";
                ws.Cell(r, 10).Value = s.Purchase_price;
                ws.Cell(r, 11).Value = s.Selling_price; // да, может быть кир. 'с' в имени в БД — на экспорт не влияет
                ws.Cell(r, 12).Value = s.Count;
                ws.Cell(r, 13).Value = s.Comment ?? "";
                r++;
            }

            ws.Range(1, 1, 1, headers.Length).Style.Font.SetBold();
            ws.Columns().AdjustToContents();
            ws.Column(6).Style.DateFormat.Format = "yyyy-MM-dd";
            ws.Column(8).Style.DateFormat.Format = "yyyy-MM-dd";
            wb.SaveAs(filePath);
        }

        //поставка
        public static void ImportSkladDeltaFromExcel(APIClass api, string filePath)
        {
            using var wb = new XLWorkbook(filePath);
            var ws = wb.Worksheets.First();

            // Подгружаем справочники один раз
            var tovars = api.TovarList() ?? new List<Tovar>();
            var sklads = api.SkladList() ?? new List<Sklad>();
            var mans = api.ManufacturerList() ?? new List<Manufacturer>();

            // Индексы
            var tovarById = tovars.ToDictionary(t => t.Tovar_id);
            var skladByTid = sklads.ToDictionary(s => s.Tovar_id);
            var manByName = mans.ToDictionary(m => (m.Name_Company ?? "").Trim().ToLowerInvariant(), m => m);
            var manById = mans.ToDictionary(m => m.Manufacturer_id, m => m);

            // Ключи для поиска товара
            static string KeyFull(int artikul, string name, int manufId)
                => $"{artikul}@@{(name ?? "").Trim().ToLowerInvariant()}@@{manufId}";
            static string KeyLoose(int artikul, string name)
                => $"{artikul}@@{(name ?? "").Trim().ToLowerInvariant()}";


            #region--- СОЗДАЁМ ОДНУ ЗАПИСЬ В HISTORY НА ВЕСЬ ИМПОРТ ---
            var addHistRes = api.AddHistory(new History { Date = DateTime.Now.ToUniversalTime() });
            if (addHistRes != "Успех")
                throw new InvalidOperationException("Не удалось создать History: " + addHistRes);

            // Получаем ID созданной записи (берём с максимальным History_id)
            var histories = api.HistoryList() ?? new List<History>();
            var history = histories.OrderByDescending(h => h.History_id).FirstOrDefault();
            if (history == null) throw new InvalidOperationException("Созданная запись History не найдена.");
            int historyId = history.History_id;
            #endregion



            // Два словаря: с производителем и без
            var tovarByKeyFull = tovars
                .GroupBy(t => KeyFull(t.Artikul, t.Name, t.id_Manufacturer))
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending<Tovar, DateTime>(t => t.Production_date == default ? DateTime.MinValue : t.Production_date)
                          .ThenByDescending<Tovar, DateTime>(t => t.Valid_until ?? DateTime.MinValue)
                          .ThenByDescending<Tovar, int>(t => t.Tovar_id)
                          .First()
                );

            var tovarByKeyLoose = tovars
                .GroupBy(t => KeyLoose(t.Artikul, t.Name))
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending<Tovar, DateTime>(t => t.Production_date == default ? DateTime.MinValue : t.Production_date)
                          .ThenByDescending<Tovar, DateTime>(t => t.Valid_until ?? DateTime.MinValue)
                          .ThenByDescending<Tovar, int>(t => t.Tovar_id)
                          .First()
                );

            // Карта заголовков
            var map = BuildHeaderMap(ws);
            int row = 2;

            while (!RowIsCompletelyEmpty(ws, row))
            {
                int? tovarId = TryGetInt(ws, row, map, "Tovar_Id");
                int delta = TryGetInt(ws, row, map, "Остаток") ?? 0;      // дельта к складу
                int artikul = TryGetInt(ws, row, map, "Артикул") ?? 0;
                string name = TryGetString(ws, row, map, "Название") ?? "";
                string type = TryGetString(ws, row, map, "Тип") ?? "";
                // Новое: читаем НАЗВАНИЕ производителя (а не id)
                string manufName = TryGetString(ws, row, map, "Производитель") ?? "";
                // На случай старых файлов оставим поддержку числового ID
                int? manufIdFromFile = TryGetInt(ws, row, map, "Производитель_Id");

                int purch = TryGetInt(ws, row, map, "Закуп_цена") ?? 0;
                int retail = TryGetInt(ws, row, map, "Розничная_цена") ?? 0;
                string unit = TryGetString(ws, row, map, "Ед_изм") ?? "шт.";
                string comm = TryGetString(ws, row, map, "Комментарий") ?? "";

                DateTime prodDate = (TryGetDate(ws, row, map, "Дата_производства")?.ToDateTime(TimeOnly.MinValue)) ?? DateTime.MinValue;
                int? warranty = TryGetInt(ws, row, map, "Гарантия_мес");
                DateTime? valid = TryGetDate(ws, row, map, "Годен_до")?.ToDateTime(TimeOnly.MinValue);

                if (delta <= 0) { row++; continue; } // нечего добавлять

                // ---- 1) Определяем/создаём производителя по имени ----
                int resolvedManufId = 0;
                if (!string.IsNullOrWhiteSpace(manufName))
                {
                    var keyName = manufName.Trim().ToLowerInvariant();
                    if (!manByName.TryGetValue(keyName, out var man))
                    {
                        // Создаём производителя (минимально валидные поля)
                        var newMan = new Manufacturer
                        {
                            Name_Company = manufName,
                            FIO_director = "—",
                            Address = "—",
                            Email = "noreply@example.com"
                        };
                        var addManRes = api.AddManufacturer(newMan);
                        if (addManRes != "Успех")
                            throw new InvalidOperationException("Не удалось создать Manufacturer: " + addManRes);

                        // Обновляем кэш производителей
                        mans = api.ManufacturerList() ?? new List<Manufacturer>();
                        manByName = mans.ToDictionary(m => (m.Name_Company ?? "").Trim().ToLowerInvariant(), m => m);
                        manById = mans.ToDictionary(m => m.Manufacturer_id, m => m);

                        if (!manByName.TryGetValue(keyName, out man))
                            throw new InvalidOperationException("Созданный Manufacturer не найден повторно.");
                    }
                    resolvedManufId = man.Manufacturer_id;
                }
                else if (manufIdFromFile.HasValue && manById.TryGetValue(manufIdFromFile.Value, out var man2))
                {
                    resolvedManufId = man2.Manufacturer_id;
                }
                // иначе оставляем 0 — товар создастся с 0, либо совпадёт по loose-ключу

                // ---- 2) Ищем существующий товар ----
                Tovar targetTovar = null;

                // По Tovar_Id
                if (tovarId.HasValue && tovarById.TryGetValue(tovarId.Value, out var tt))
                {
                    targetTovar = tt;
                }
                else
                {
                    // Сначала пытаемся полным ключом (если знаем производителя)
                    if (resolvedManufId != 0)
                    {
                        var keyFull = KeyFull(artikul, name, resolvedManufId);
                        if (!tovarByKeyFull.TryGetValue(keyFull, out targetTovar))
                        {
                            // нет — попробуем loose-ключом
                            var keyLoose = KeyLoose(artikul, name);
                            tovarByKeyLoose.TryGetValue(keyLoose, out targetTovar);
                        }
                    }
                    else
                    {
                        // Производителя не знаем — сразу по loose-ключу
                        var keyLoose = KeyLoose(artikul, name);
                        tovarByKeyLoose.TryGetValue(keyLoose, out targetTovar);
                    }
                }

                // ---- 3) Если не нашли — создаём Tovar ----
                if (targetTovar == null)
                {
                    var newTovar = new Tovar
                    {
                        Artikul = artikul,
                        Name = name,
                        Type_tovar = type,
                        id_Manufacturer = resolvedManufId,
                        Production_date = prodDate,
                        Manufacturer_warranty = warranty,
                        Valid_until = valid
                    };

                    var res1 = api.AddTovar(newTovar);
                    if (res1 != "Успех")
                        throw new InvalidOperationException("Не удалось создать Tovar: " + res1);

                    // Обновим кэш товаров и словари
                    tovars = api.TovarList() ?? new List<Tovar>();
                    tovarById = tovars.ToDictionary(t => t.Tovar_id);

                    tovarByKeyFull = tovars
                        .GroupBy(t => KeyFull(t.Artikul, t.Name, t.id_Manufacturer))
                        .ToDictionary(
                            g => g.Key,
                            g => g.OrderByDescending<Tovar, DateTime>(t => t.Production_date == default ? DateTime.MinValue : t.Production_date)
                                  .ThenByDescending<Tovar, DateTime>(t => t.Valid_until ?? DateTime.MinValue)
                                  .ThenByDescending<Tovar, int>(t => t.Tovar_id)
                                  .First()
                        );

                    tovarByKeyLoose = tovars
                        .GroupBy(t => KeyLoose(t.Artikul, t.Name))
                        .ToDictionary(
                            g => g.Key,
                            g => g.OrderByDescending<Tovar, DateTime>(t => t.Production_date == default ? DateTime.MinValue : t.Production_date)
                                  .ThenByDescending<Tovar, DateTime>(t => t.Valid_until ?? DateTime.MinValue)
                                  .ThenByDescending<Tovar, int>(t => t.Tovar_id)
                                  .First()
                        );

                    // Возьмём только что созданный
                    var kFull = KeyFull(artikul, name, resolvedManufId);
                    if (!tovarByKeyFull.TryGetValue(kFull, out targetTovar))
                    {
                        // на всякий случай попробуем loose
                        var kLoose = KeyLoose(artikul, name);
                        if (!tovarByKeyLoose.TryGetValue(kLoose, out targetTovar))
                            throw new InvalidOperationException("Созданный Tovar не найден повторно.");
                    }
                }

                // ---- 4) Обновляем/создаём запись склада ----
                if (skladByTid.TryGetValue(targetTovar.Tovar_id, out var skl))
                {
                    skl.Count += delta;
                    if (purch > 0) skl.Purchase_price = purch;
                    if (retail > 0) skl.Selling_price = retail;
                    if (!string.IsNullOrWhiteSpace(unit)) skl.unit = unit;
                    if (!string.IsNullOrWhiteSpace(comm)) skl.Comment = comm;

                    var res2 = api.UpdateSklad(skl);
                    if (res2 != "Успех")
                        throw new InvalidOperationException("Не удалось обновить Sklad: " + res2);
                }
                else
                {
                    var newSklad = new Sklad
                    {
                        Tovar_id = targetTovar.Tovar_id,
                        Count = delta,
                        Purchase_price = purch,
                        Selling_price = retail,
                        unit = unit,
                        Comment = comm
                    };
                    var res3 = api.AddSklad(newSklad);
                    if (res3 != "Успех")
                        throw new InvalidOperationException("Не удалось создать Sklad: " + res3);

                    // обновим кеш склада
                    sklads = api.SkladList() ?? new List<Sklad>();
                    skladByTid = sklads.ToDictionary(s => s.Tovar_id);
                }

                // ---- 5) ДОБАВЛЯЕМ СТРОКУ В SHIPMENT ДЛЯ ЭТОЙ ПОЗИЦИИ ----
                var ship = new Shipment
                {
                    History_id = historyId,
                    Tovar_id = targetTovar.Tovar_id,
                    Unit = unit,
                    Count = delta,
                    Purchase_price = purch
                };
                var resShip = api.AddShipment(ship);
                if (resShip != "Успех")
                    throw new InvalidOperationException("Не удалось создать Shipment: " + resShip);

                row++;
            }
        }


        //новый товар
        public static void ImportNewTovarsFromExcel(APIClass api, string filePath)
        {
            using var wb = new XLWorkbook(filePath);
            var ws = wb.Worksheets.First();

            // 1) Справочники из API
            var mans = api.ManufacturerList() ?? new List<Manufacturer>();
            var tovars = api.TovarList() ?? new List<Tovar>();

            // Индексы производителей
            static string Norm(string s) => (s ?? "").Trim().ToLowerInvariant();
            var manByName = mans.ToDictionary(m => Norm(m.Name_Company), m => m);
            var manById = mans.ToDictionary(m => m.Manufacturer_id, m => m);

            // Ключ товара: Артикул+Название+ПроизводительId
            static string KeyFull(int artikul, string name, int manufId)
                => $"{artikul}@@{(name ?? "").Trim().ToLowerInvariant()}@@{manufId}";

            // Локальный индекс существующих товаров
            var tovarByKeyFull = tovars
                .GroupBy(t => KeyFull(t.Artikul, t.Name, t.id_Manufacturer))
                .ToDictionary(g => g.Key, g => g.OrderByDescending(t => t.Tovar_id).First());

            // 2) Заголовки
            var map = BuildHeaderMap(ws);
            int row = 2;

            while (!RowIsCompletelyEmpty(ws, row))
            {
                // --- читаем значения через ТВОИ TryGet* ---
                int artikul = TryGetInt(ws, row, map, "Артикул") ?? 0;
                string name = TryGetString(ws, row, map, "Название") ?? "";
                string type = TryGetString(ws, row, map, "Тип") ?? "";

                // Производителя читаем по ИМЕНИ (колонка "Производитель"); для совместимости оставим "Производитель_Id"
                string manufName = TryGetString(ws, row, map, "Производитель") ?? "";
             //   int? manufIdOld = TryGetInt(ws, row, map, "Производитель_Id");

                var prodDateDo = TryGetDate(ws, row, map, "Дата_производства"); // DateOnly?
                int? warranty = TryGetInt(ws, row, map, "Гарантия_мес");
                var validDo = TryGetDate(ws, row, map, "Годен_до");          // DateOnly?

                string unit = TryGetString(ws, row, map, "Ед_изм") ?? "шт.";
                int purch = TryGetInt(ws, row, map, "Закуп_цена") ?? 0;
                int retail = TryGetInt(ws, row, map, "Розничная_цена") ?? 0;
                int countInit = TryGetInt(ws, row, map, "Остаток") ?? 0;
                string comment = TryGetString(ws, row, map, "Комментарий") ?? "";

                // Пропускаем пустые ключевые строки
                if (artikul == 0 || string.IsNullOrWhiteSpace(name)) { row++; continue; }

                // --- 3) Разруливаем производителя по имени (или по старому Id) ---
                int resolvedManufId = 0;
                if (!string.IsNullOrWhiteSpace(manufName))
                {
                    var key = Norm(manufName);
                    if (!manByName.TryGetValue(key, out var man))
                    {
                        // создаём минимально валидного производителя
                        var newMan = new Manufacturer
                        {
                            Name_Company = manufName,
                            FIO_director = "—",
                            Address = "—",
                            Email = "noreply@example.com"
                        };
                        var addManRes = api.AddManufacturer(newMan);
                        if (addManRes != "Успех")
                            throw new InvalidOperationException("Не удалось создать Manufacturer: " + addManRes);

                        // обновляем кэш
                        mans = api.ManufacturerList() ?? new List<Manufacturer>();
                        manByName = mans.ToDictionary(m => Norm(m.Name_Company), m => m);
                        manById = mans.ToDictionary(m => m.Manufacturer_id, m => m);

                        man = manByName[key];
                    }
                    resolvedManufId = man.Manufacturer_id;
                }
               

                // --- 4) Проверка на существование товара (строгий ключ) ---
                var keyFull = KeyFull(artikul, name, resolvedManufId);
                if (tovarByKeyFull.ContainsKey(keyFull))
                {
                    // уже есть — пропускаем строку
                    row++;
                    continue;
                }

                // --- 5) Создаём Tovar ---
                var newTovar = new Tovar
                {
                    Artikul = artikul,
                    Name = name,
                    Type_tovar = type,
                    id_Manufacturer = resolvedManufId,
                    Production_date = prodDateDo?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                    Manufacturer_warranty = warranty,
                    Valid_until = validDo?.ToDateTime(TimeOnly.MinValue)
                };
                var addTRes = api.AddTovar(newTovar);
                if (addTRes != "Успех")
                    throw new InvalidOperationException("Не удалось создать Tovar: " + addTRes);

                // Обновляем список и находим созданного
                tovars = api.TovarList() ?? new List<Tovar>();
                var created = tovars
                    .OrderByDescending(t => t.Tovar_id)
                    .FirstOrDefault(t =>
                        t.Artikul == artikul &&
                        Norm(t.Name) == Norm(name) &&
                        t.id_Manufacturer == resolvedManufId);
                if (created == null)
                    throw new InvalidOperationException("Созданный Tovar не найден повторно.");

                // Добавим в локальный индекс, чтобы не создать дубль в рамках одного файла
                tovarByKeyFull[keyFull] = created;

                // --- 6) Создаём Sklad (стартовый остаток/цены/ед.) ---
                var newSklad = new Sklad
                {
                    Tovar_id = created.Tovar_id,
                    Count = Math.Max(0, countInit),
                    Purchase_price = purch,
                    Selling_price = retail,   
                    unit = unit,
                    Comment = comment
                };
                var addSRes = api.AddSklad(newSklad);
                if (addSRes != "Успех")
                    throw new InvalidOperationException("Не удалось создать Sklad: " + addSRes);

                row++;
            }
        }



        // ====== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ======

        private static Dictionary<string, int> BuildHeaderMap(IXLWorksheet ws)
        {
            // Синонимы заголовков -> "каноническое" имя
            var headerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "№", "№" },
                { "N", "№" },
                { "No", "№" },
                { "Артикул", "Артикул" },
                { "Название", "Название" },
                { "Наименование", "Название" },
                { "Производитель", "Производитель" },
                { "Производитель_id", "Производитель" },
                { "Дата_производства", "Дата_производства" },
                { "Дата производства", "Дата_производства" },
                { "Тип", "Тип" },
                { "Тип_товара", "Тип" },
                { "Гарантия_мес", "Гарантия_мес" },
                { "Гарантия", "Гарантия_мес" },
                { "Годен_до", "Годен_до" },
                { "Годен до", "Годен_до" },
                { "Ед_изм", "Ед_изм" },
                { "Единица", "Ед_изм" },
                { "Закуп_цена", "Закуп_цена" },
                { "Закупочная цена", "Закуп_цена" },
                { "Розничная_цена", "Розничная_цена" },
                { "Selling_price", "Розничная_цена" },             
                { "Остаток", "Остаток" },
                { "Count", "Остаток" },
                { "Комментарий", "Комментарий" }
            };

            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            int col = 1;
            while (!ws.Cell(1, col).IsEmpty())
            {
                var raw = ws.Cell(1, col).GetString().Trim();

                // Маппим синоним к каноническому названию
                if (headerMap.TryGetValue(raw, out var canon))
                {
                    if (!map.ContainsKey(canon))
                        map[canon] = col;
                }
                else
                {
                    // Если неизвестный — пробуем брать как есть
                    if (!map.ContainsKey(raw))
                        map[raw] = col;
                }

                col++;
            }

            return map;
        }


        private static bool RowIsCompletelyEmpty(IXLWorksheet ws, int row)
        {
            // Считаем пустой, если первые, скажем, 6 ячеек пустые
            for (int c = 1; c <= 6; c++)
                if (!ws.Cell(row, c).IsEmpty()) return false;
            return true;
        }

        private static string? TryGetString(IXLWorksheet ws, int row, Dictionary<string, int> map, string key)
        {
            if (!map.TryGetValue(key, out var col)) return null;
            var v = ws.Cell(row, col).GetString();
            return string.IsNullOrWhiteSpace(v) ? null : v.Trim();
        }

        private static int? TryGetInt(IXLWorksheet ws, int row, Dictionary<string, int> map, string key)
        {
            if (!map.TryGetValue(key, out var col)) return null;

            var cell = ws.Cell(row, col);
            if (cell.DataType == XLDataType.Number)
                return (int)cell.GetDouble();

            var s = cell.GetString().Trim();
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var val))
                return val;

            return null;
        }

        private static DateOnly? TryGetDate(IXLWorksheet ws, int row, Dictionary<string, int> map, string key)
        {
            if (!map.TryGetValue(key, out var col)) return null;
            var cell = ws.Cell(row, col);

            if (cell.DataType == XLDataType.DateTime)
                return DateOnly.FromDateTime(cell.GetDateTime());

            var s = cell.GetString().Trim();
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt))
                return DateOnly.FromDateTime(dt);

            return null;
        }
    }
}

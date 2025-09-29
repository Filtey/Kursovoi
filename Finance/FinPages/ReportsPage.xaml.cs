using ClosedXML.Excel;
using StoreSystem.ConnectToDB.Model;
using StoreSystem.ConnectToDB.Model.ApiCRUDs;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

// !!! подключи пространство имён, где лежат модели: Sell, SellTovars, Account, Tovar, Sklad, Shipment, History
// using StoreSystem.ConnectToDB.Model;  // пример

namespace StoreSystem.Finance.FinPages
{
    public partial class ReportsPage : Page
    {
        private readonly APIClass _api = new APIClass();

        // кэш сырых данных из API
        private List<Sell> _sells;
        private List<SellTovars> _sellTovars;
        private List<Account> _accounts;
        private List<Tovar> _tovars;
        private List<Sklad> _sklad;
        private List<Shipment> _shipments;
        private List<History> _history;

        private DataTable _current = new DataTable();

        public ReportsPage()
        {
            InitializeComponent();
            try
            {
                // период по умолчанию — текущий месяц
                var now = DateTime.Now;
                dpFrom.SelectedDate = new DateTime(now.Year, now.Month, 1);
                dpTo.SelectedDate = dpFrom.SelectedDate.Value.AddMonths(1).AddDays(-1);

                //   _ = GenerateAsync(); // первичная загрузка
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        #region UI handlers
        private async void Generate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleParamsByReport();
                await GenerateAsync();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private async void Preset_Today_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var d = DateTime.Today;
                dpFrom.SelectedDate = d; dpTo.SelectedDate = d;
                await GenerateAsync();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        private async void Preset_ThisWeek_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var today = DateTime.Today;
                var start = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
                var end = start.AddDays(6);
                dpFrom.SelectedDate = start; dpTo.SelectedDate = end;
                await GenerateAsync();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        private async void Preset_ThisMonth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var now = DateTime.Today;
                var start = new DateTime(now.Year, now.Month, 1);
                var end = start.AddMonths(1).AddDays(-1);
                dpFrom.SelectedDate = start; dpTo.SelectedDate = end;
                await GenerateAsync();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        private void ToggleParamsByReport()
        {
            try
            {
                var tag = (cbReportType.SelectedItem as ComboBoxItem)?.Tag?.ToString();
                tbTopN.IsEnabled = tag == "TopSku";
                tbHorizonDays.IsEnabled = tag == "ExpiringValidity" || tag == "SlowMovers";
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }


        private void Export_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_current == null || _current.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта.", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dlg = new SaveFileDialog
                {
                    Filter = "Книга Excel (*.xlsx)|*.xlsx",
                    FileName = $"Отчёт_{GetReportTitle().Replace(' ', '_')}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
                };
                if (dlg.ShowDialog() != true) return;

                try
                {
                    using var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add("Отчёт");

                    // заголовок
                    ws.Cell(1, 1).Value = GetReportTitle();
                    ws.Range(1, 1, 1, Math.Max(1, _current.Columns.Count)).Merge()
                        .Style.Font.SetBold().Font.SetFontSize(14)
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    ws.Cell(2, 1).Value = $"Период: {dpFrom.SelectedDate:dd.MM.yyyy} — {dpTo.SelectedDate:dd.MM.yyyy}";
                    ws.Range(2, 1, 2, Math.Max(1, _current.Columns.Count)).Merge();

                    // вставка таблицы
                    ws.Cell(4, 1).InsertTable(_current, true);

                    // автоформатирование столбцов
                    var used = ws.RangeUsed();
                    if (used != null)
                    {
                        foreach (var col in used.Columns())
                        {
                            var header = col.FirstCell().GetValue<string>().ToLowerInvariant();

                            // формат даты
                            if (header.Contains("дата") || header.Contains("день") || header.Contains("годен") || header.Contains("продажа"))
                            {
                                foreach (var cell in col.CellsUsed().Skip(1))
                                    cell.Style.DateFormat.Format = "dd.MM.yyyy";
                            }

                            // формат денег / сумм / цен
                            if (header.Contains("выруч") || header.Contains("сумм") ||
                                header.Contains("оценка") || header.Contains("стоим") || header.Contains("цен"))
                            {
                                foreach (var cell in col.CellsUsed().Skip(1))
                                    cell.Style.NumberFormat.Format = "#,##0.00";
                            }
                        }

                        ws.Columns().AdjustToContents();
                    }
                    wb.SaveAs(dlg.FileName);
                    MessageBox.Show("Файл успешно сохранён.", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка экспорта: " + ex.Message, "Экспорт", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        #endregion

        private string GetReportTitle()
        {
            try { return (cbReportType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Отчёт"; }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return "";
            }
        }

        private (DateTime? from, DateTime? to, int topN, int horizonDays, string tag) ReadFilters()
        {
            try
            {
                if (dpFrom == null || dpTo == null || tbTopN == null || tbHorizonDays == null || cbReportType == null)
                {
                    return (DateTime.Now, DateTime.Now, 20, 60, "SalesByDay");
                }
                var from = dpFrom.SelectedDate?.Date;
                var to = dpTo.SelectedDate?.Date;
                int.TryParse(tbTopN.Text, out var topN); if (topN <= 0) topN = 20;
                int.TryParse(tbHorizonDays.Text, out var horiz); if (horiz <= 0) horiz = 60;
                var tag = (cbReportType.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "SalesByDay";
                return (from, to, topN, horiz, tag);
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return (null, null, 0, 0, null);
            }
        }

        private async Task GenerateAsync()
        {
            try
            {
                if (dg == null) return;
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                _sells      = _api.SellList()       ?? new List<Sell>();
                _sellTovars = _api.SellTovarsList() ?? new List<SellTovars>();
                _accounts   = _api.AccountList()    ?? new List<Account>();
                _tovars     = _api.TovarList()      ?? new List<Tovar>();
                _sklad      = _api.SkladList()      ?? new List<Sklad>();
                _shipments  = _api.ShipmentList()   ?? new List<Shipment>();
                _history    = _api.HistoryList()    ?? new List<History>();
              //  await EnsureDataLoadedAsync();

                var (from, to, topN, horizonDays, tag) = ReadFilters();

                // строим отчёт
                _current = tag switch
                {
                    "SalesByDay" => BuildSalesByDay(from, to),
                    "SalesByCashier" => BuildSalesByCashier(from, to),
                    "TopSku" => BuildTopSku(from, to, topN),
                    "StockValuation" => BuildStockValuation(),
                    "Shipments" => BuildShipments(from, to),
                    "ExpiringValidity" => BuildExpiringValidity(horizonDays),
                    "SlowMovers" => BuildSlowMovers(horizonDays),
                    "PriceAnomalies" => BuildPriceAnomalies(),
                    _ => new DataTable()
                };


                dg.ItemsSource = _current.DefaultView;
                ApplyGridColumnFormats();
                tbCount.Text = $"Результатов: {_current.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка формирования отчёта:\n" + ex.Message, "Отчёты", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }



        /// <summary>
        /// Форматирует авто-сгенерированные колонки DataGrid по заголовку:
        /// даты → dd.MM.yyyy, деньги/суммы/цены → N2, цифры выравниваем вправо.
        /// </summary>
        private void ApplyGridColumnFormats()
        {
            try
            {
                if (dg?.Columns == null) return;

                // стиль выравнивания вправо для числовых колонок
                var rightAlign = new Style(typeof(TextBlock));
                rightAlign.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));

                foreach (var col in dg.Columns.OfType<DataGridTextColumn>())
                {
                    var header = (col.Header?.ToString() ?? "").ToLowerInvariant();

                    // достаём Binding и задаём формат
                    if (col.Binding is Binding b)
                    {
                        // даты
                        if (header.Contains("дата") || header.Contains("день") || header.Contains("годен") || header.Contains("продажа"))
                        {
                            b.StringFormat = "dd.MM.yyyy";
                        }

                        // деньги / суммы / цены / оценка / стоимость
                        if (header.Contains("выруч") || header.Contains("сумм") ||
                            header.Contains("оценка") || header.Contains("стоим") || header.Contains("цена"))
                        {
                            b.StringFormat = "N2";
                            col.ElementStyle = rightAlign;
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }


        #region Helpers: общие словари и доступ к полям
        private decimal GetSellingPriceOrZero(int tovarId)
        {
            try
            {

                var rec = _sklad.FirstOrDefault(x => x.Tovar_id == tovarId);
                if (rec == null) return 0m;

                // Попробуем сначала рефлексией (на случай неудобного имени)
                var prop = rec.GetType().GetProperty("Selling_priсe") ?? rec.GetType().GetProperty("Selling_price");
                if (prop != null)
                {
                    var val = prop.GetValue(rec);
                    if (val == null) return 0m;
                    if (val is decimal d) return d;
                    if (decimal.TryParse(val.ToString(), out var p)) return p;
                }
                return 0m;
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return 0;
            }
        }

        private decimal GetPurchasePriceOrZero(Shipment sh)
        {
            try
            {
                var prop = sh.GetType().GetProperty("Purchase_price");
                if (prop == null) return 0m;
                var v = prop.GetValue(sh);
                if (v == null) return 0m;
                if (v is decimal d) return d;
                if (decimal.TryParse(v.ToString(), out var p)) return p;
                return 0m;
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return 0;
            }
        }

        private DateTime? DateSellOf(SellTovars st)
        {
            try
            {
                var prop = st.GetType().GetProperty("Date_sell");
                if (prop?.GetValue(st) is DateTime dt) return dt.Date;
                if (DateTime.TryParse(prop?.GetValue(st)?.ToString(), out var p)) return p.Date;
                return null;
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }

        private DateTime? HistoryDateOf(History h)
        {
            try
            {
                var prop = h.GetType().GetProperty("Date");
                if (prop?.GetValue(h) is DateTime dt) return dt.Date;
                if (DateTime.TryParse(prop?.GetValue(h)?.ToString(), out var p)) return p.Date;
                return null;
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }
        #endregion

        #region Builders (8 отчётов)
        private DataTable BuildSalesByDay(DateTime? _from, DateTime? to)
        {
            try
            {
                var join = from s in _sells
                           join st in _sellTovars on s.SellTovars_id equals st.SellTovars_id
                           let d = DateSellOf(st)
                           where (!_from.HasValue || (d.HasValue && d.Value >= _from.Value))
                              && (!to.HasValue || (d.HasValue && d.Value <= to.Value))
                           select new
                           {
                               Day = d,
                               ReceiptId = st.SellTovars_id,
                               Items = s.Count,
                               Amount = s.Count * GetSellingPriceOrZero(s.Tovar_id)
                           };

                var grouped = join
                    .GroupBy(x => x.Day)
                    .Select(g => new
                    {
                        День = g.Key,
                        Чеков = g.Select(x => x.ReceiptId).Distinct().Count(),
                        Товаров = g.Sum(x => x.Items),
                        Выручка = g.Sum(x => x.Amount),
                        Средний_чек = g.Select(x => x.ReceiptId).Distinct().Any()
                            ? g.Sum(x => x.Amount) / g.Select(x => x.ReceiptId).Distinct().Count()
                            : 0m
                    })
                    .OrderBy(x => x.День)
                    .ToList();

                return ToDataTable(grouped);
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }

        private DataTable BuildSalesByCashier(DateTime? _from, DateTime? to)
        {
            try
            {
                var accountsById = _accounts.ToDictionary(a => a.Account_id, a =>
                    $"{a.Surname} {a.Name}{(string.IsNullOrWhiteSpace(a.Patronymic) ? "" : " " + a.Patronymic)}");

                var join = from s in _sells
                           join st in _sellTovars on s.SellTovars_id equals st.SellTovars_id
                           let d = DateSellOf(st)
                           where (!_from.HasValue || (d.HasValue && d.Value >= _from.Value))
                              && (!to.HasValue || (d.HasValue && d.Value <= to.Value))
                           let cashier = accountsById.TryGetValue(st.Kassir_id, out var fio) ? fio : "—"
                           select new
                           {
                               Cashier = cashier,
                               ReceiptId = st.SellTovars_id,
                               Items = s.Count,
                               Amount = s.Count * GetSellingPriceOrZero(s.Tovar_id)
                           };

                var grouped = join
                    .GroupBy(x => x.Cashier)
                    .Select(g => new
                    {
                        Кассир = g.Key,
                        Чеков = g.Select(x => x.ReceiptId).Distinct().Count(),
                        Товаров = g.Sum(x => x.Items),
                        Выручка = g.Sum(x => x.Amount)
                    })
                    .OrderByDescending(x => x.Выручка)
                    .ThenBy(x => x.Кассир)
                    .ToList();

                return ToDataTable(grouped);
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }

        private DataTable BuildTopSku(DateTime? _from, DateTime? to, int topN)
        {
            try
            {
                var tovarById = _tovars.ToDictionary(t => t.Tovar_id, t => new { t.Name, t.Artikul });

                var join = from s in _sells
                           join st in _sellTovars on s.SellTovars_id equals st.SellTovars_id
                           let d = DateSellOf(st)
                           where (!_from.HasValue || (d.HasValue && d.Value >= _from.Value))
                              && (!to.HasValue || (d.HasValue && d.Value <= to.Value))
                           let sku = tovarById.TryGetValue(s.Tovar_id, out var tv) ? tv : new { Name = "—", Artikul = 0 }
                           select new
                           {
                               Artikul = sku.Artikul,
                               Tovar = sku.Name,
                               Qty = s.Count,
                               Amount = s.Count * GetSellingPriceOrZero(s.Tovar_id)
                           };

                var rows = join
                    .GroupBy(x => new { x.Artikul, x.Tovar })
                    .Select(g => new
                    {
                        Артикул = g.Key.Artikul,
                        Товар = g.Key.Tovar,
                        Кол_во = g.Sum(x => x.Qty),
                        Выручка = g.Sum(x => x.Amount)
                    })
                    .OrderByDescending(x => x.Выручка)
                    .Take(topN)
                    .ToList();

                return ToDataTable(rows);
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }

        private DataTable BuildStockValuation()
        {
            try
            {
                var rows = from sk in _sklad
                           join t in _tovars on sk.Tovar_id equals t.Tovar_id
                           let count = sk.Count
                           let buy = GetPropertyDecimal(sk, "Purchase_price")
                           let sell = GetPropertyDecimal(sk, "Selling_priсe") ?? GetPropertyDecimal(sk, "Selling_price") ?? 0m
                           select new
                           {
                               Артикул = t.Artikul,
                               Товар = t.Name,
                               Остаток = count,
                               Закупочная_цена = buy,
                               Розничная_цена = sell,
                               Оценка_по_закупке = (count * (buy ?? 0m)),
                               Оценка_по_рознице = (count * sell)
                           };

                return ToDataTable(rows
                    .OrderByDescending(x => x.Оценка_по_рознице)
                    .ToList());
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }

        private DataTable BuildShipments(DateTime? from, DateTime? to)
        {
            try
            {
                var histById = _history.ToDictionary(h => h.History_id, h => HistoryDateOf(h));
                var tovarById = _tovars.ToDictionary(t => t.Tovar_id, t => new { t.Name, t.Artikul });

                var rows = _shipments
                    .Select(sh => new
                    {
                        Date = histById.TryGetValue(sh.History_id, out var d) ? d : (DateTime?)null,
                        sh.Tovar_id,
                        sh.Count,
                        Purchase = GetPurchasePriceOrZero(sh)
                    })
                    .Where(x => x.Date.HasValue
                                && (!from.HasValue || x.Date.Value >= from.Value)
                                && (!to.HasValue || x.Date.Value <= to.Value))
                    .Select(x => new
                    {
                        Дата = x.Date.Value,
                        Артикул = tovarById.TryGetValue(x.Tovar_id, out var tv) ? tv.Artikul : 0,
                        Товар = tovarById.TryGetValue(x.Tovar_id, out var tv2) ? tv2.Name : "—",
                        Кол_во = x.Count,
                        Закупочная_цена = x.Purchase,
                        Сумма = x.Purchase * x.Count
                    })
                    .OrderBy(r => r.Дата).ThenBy(r => r.Товар)
                    .ToList();

                return ToDataTable(rows);
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }

        private DataTable BuildExpiringValidity(int horizonDays)
        {
            try
            {
                var rows = _tovars
                    .Where(t => t.Valid_until != null)
                    .Select(t => new
                    {
                        t.Artikul,
                        t.Name,
                        ValidUntil = t.Valid_until.Value.Date,
                        DaysLeft = (t.Valid_until.Value.Date - DateTime.Today).Days
                    })
                    .Where(x => x.DaysLeft <= horizonDays)
                    .OrderBy(x => x.ValidUntil)
                    .Select(x => new
                    {
                        Артикул = x.Artikul,
                        Товар = x.Name,
                        Годен_до = x.ValidUntil,
                        Осталось_дней = x.DaysLeft
                    })
                    .ToList();

                return ToDataTable(rows);
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }

        private DataTable BuildSlowMovers(int noSalesDays)
        {
            try
            {
                var lastSaleByTovar = (from s in _sells
                                       join st in _sellTovars on s.SellTovars_id equals st.SellTovars_id
                                       let d = DateSellOf(st)
                                       where d.HasValue
                                       group d by s.Tovar_id into g
                                       select new { TovarId = g.Key, Last = g.Max().Value.Date })
                                       .ToDictionary(x => x.TovarId, x => x.Last);

                var rows = (from sk in _sklad
                            join t in _tovars on sk.Tovar_id equals t.Tovar_id
                            let last = lastSaleByTovar.TryGetValue(t.Tovar_id, out var d) ? (DateTime?)d : null
                            let days = (last.HasValue ? (DateTime.Today - last.Value).Days : int.MaxValue)
                            where sk.Count > 0 && (last == null || days >= noSalesDays)
                            select new
                            {
                                Артикул = t.Artikul,
                                Товар = t.Name,
                                Остаток = sk.Count,
                                Последняя_продажа = last,
                                Дней_без_продаж = (last == null ? null : (int?)days) ?? null as int?
                            })
                            .OrderByDescending(x => x.Дней_без_продаж ?? int.MaxValue)
                            .ThenByDescending(x => x.Остаток)
                            .ToList();

                // для null дней сделаем отображение как пусто — DataTable сам справится
                return ToDataTable(rows);
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }

        private DataTable BuildPriceAnomalies()
        {
            try
            {
                var rows = from sk in _sklad
                           join t in _tovars on sk.Tovar_id equals t.Tovar_id
                           let sell = GetPropertyDecimal(sk, "Selling_priсe") ?? GetPropertyDecimal(sk, "Selling_price")
                           where !sell.HasValue || sell.Value <= 0m
                           select new
                           {
                               Артикул = t.Artikul,
                               Товар = t.Name,
                               Розничная_цена = sell
                           };

                return ToDataTable(rows.OrderBy(x => x.Товар).ToList());
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
               return null;
            }
        }
        #endregion

        #region Small utilities
        private static DataTable ToDataTable<T>(IEnumerable<T> items)
        {
            try
            {
                var dt = new DataTable();
                if (items == null) return dt;

                var first = items.FirstOrDefault();
                if (first == null) return dt;

                var props = typeof(T).GetProperties();
                foreach (var p in props)
                {
                    var type = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                    dt.Columns.Add(p.Name.Replace('_', ' '), type);
                }

                foreach (var it in items)
                {
                    var row = dt.NewRow();
                    foreach (var p in props)
                    {
                        row[p.Name.Replace('_', ' ')] = p.GetValue(it) ?? DBNull.Value;
                    }
                    dt.Rows.Add(row);
                }

                return dt;
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }

        private static decimal? GetPropertyDecimal(object obj, string propertyName)
        {
            try
            {
                var prop = obj.GetType().GetProperty(propertyName);
                if (prop == null) return null;
                var v = prop.GetValue(obj);
                if (v == null) return null;
                if (v is decimal d) return d;
                if (decimal.TryParse(v.ToString(), out var p)) return p;
                return null;
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }
        #endregion
    }
}

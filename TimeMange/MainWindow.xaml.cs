using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Text.Json;
using System.IO;
using System.Dynamic;
using System.Linq;

namespace TimeMange;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const int 星期五 = 5;
    private readonly int 发薪水日子;
    private readonly DispatcherTimer 计时器 = new();
    private readonly dynamic 时间配置 = new ExpandoObject();
    private readonly Dictionary<string, DateTime> 节假日字典 = new();
    private readonly Func<int, int, DateTime> 根据月日获取时间 = (int 月, int 日)
        => new DateTime(year: 当前时间.Year,
            month: 月,
            day: 日);
    public MainWindow()
    {
        var 路径 = AppDomain.CurrentDomain.BaseDirectory;
        var 根路径索引 = 路径.LastIndexOf("TimeMange");
        var 配置JSON字符串 = File.ReadAllText(路径[..根路径索引] + @"TimeMange/config.json");

        var 配置文件对象 = JsonSerializer.Deserialize<JsonElement>(配置JSON字符串)!;

        发薪水日子 = 配置文件对象.GetProperty(nameof(发薪水日子)).GetInt32();

        (时间配置.吃饭时间小时, 时间配置.吃饭时间分钟) = 根据键获取值("吃饭时间");

        (时间配置.上班时间小时, 时间配置.上班时间分钟) = 根据键获取值("上班时间");

        (时间配置.下班时间小时, 时间配置.下班时间分钟) = 根据键获取值("下班时间");

        var 节日数组 = 配置文件对象.GetProperty("节日").EnumerateArray();
        foreach (var 节日 in 节日数组)
        {
            var 节日明细 = 节日.Deserialize<Dictionary<string, JsonElement>>()!;
            foreach (var (节日名字, 节日日期) in 节日明细)
            {
                节假日字典.Add(
                    key: 节日名字,
                    value: 根据月日获取时间(节日日期.GetProperty("月").GetInt32(),
                           节日日期.GetProperty("日").GetInt32())
                    );
            }
        }
        节假日字典 = 节假日字典.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        InitializeComponent();
        计时器.Tick += new EventHandler(计时器事件);
        计时器.Interval = TimeSpan.FromSeconds(1);
        计时器.Start();

        (int, int) 根据键获取值(string name)
        {
            var data = 配置文件对象.GetProperty(name).GetString()!.Split(':');
            return (int.Parse(data[0]), int.Parse(data[1]));
        }
    }
    private void 计时器事件(object? 发送, EventArgs 事件)
    {
        var 星期几 = (int)当前时间.DayOfWeek;
        离周五天数.Text = 补零(星期五 > 星期几 ? 5 - 星期几 : 7 - 星期几 + 5);

        if (发薪水日子 > 当前时间.Day)
        {
            离发薪天数.Text = 补零(发薪水日子 - 当前时间.Day);
        }
        else
        {
            离发薪天数.Text = 补零(DateTime.DaysInMonth(当前时间.Year, 当前时间.Month) - 当前时间.Day + 发薪水日子);
        }

        foreach (var (键, 值) in 节假日字典)
        {
            if (当前时间 < 值)
            {
                节日名称.Text = 键;
                离节日天数.Text = 补零((值 - 当前时间).Days + 1);
                break;
            }
        }

        var 时间差 = 获取时间差();
        timeSpanTxt.Text = $"{补零(时间差.Hours)}:{补零(时间差.Minutes)}:{补零(时间差.Seconds)}";
    }
    private TimeSpan 获取时间差()
    {
        TimeSpan 时间差;
        if (当前时间 < 吃饭时间)
        {
            nowTime.Text = "离吃饭还有";
            时间差 = 吃饭时间 - 当前时间;
        }
        else if (当前时间 < 下班时间)
        {
            nowTime.Text = "离下班还有";
            时间差 = 下班时间 - 当前时间;
        }
        else
        {
            nowTime.Text = "离上班还有";
            时间差 = 上班时间 - 当前时间;
        }
        return 时间差;
    }
    private static string 补零(int 数字) => 数字 < 10 ? "0" + 数字 : 数字.ToString();
    private void Window_PreviewMouseLeftButtonDown(object 发送, MouseButtonEventArgs 事件)
    {
        DragMove();
    }
    private static DateTime 当前时间 { get => DateTime.Now; }
    private DateTime 上班时间
    {
        get => new(
        year: 当前时间.Year,
        month: 当前时间.Month,
        day: 当前时间.Day + 1,
        hour: 时间配置.上班时间小时,
        minute: 时间配置.上班时间分钟,
        second: 0);
    }
    private DateTime 下班时间
    {
        get => new(year: 当前时间.Year, month: 当前时间.Month, day: 当前时间.Day, hour: 时间配置.下班时间小时, minute: 时间配置.下班时间分钟, second: 0);
    }
    private DateTime 吃饭时间
    {
        get => new(
            year: 当前时间.Year,
            month: 当前时间.Month,
            day: 当前时间.Day,
            hour: 时间配置.吃饭时间小时,
            minute: 时间配置.吃饭时间分钟,
            second: 0);
    }
    //private (DateTime, DateTime) 喝水提醒
    //{
    //    get => (new DateTime(
    //        year: 当前时间.Year,
    //        month: 当前时间.Month,
    //        day: 当前时间.Day,
    //        hour: 8,
    //        minute: 0,
    //        second: 0), new DateTime(
    //        year: 当前时间.Year,
    //        month: 当前时间.Month,
    //        day: 当前时间.Day,
    //        hour: 17,
    //        minute: 0,
    //        second: 0));
    //}
}

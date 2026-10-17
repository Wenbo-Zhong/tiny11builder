using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Tiny11Builder.App;

// Simplified Chinese overlay. English strings in upstream XAML and view-models stay the source of
// truth, so an upstream merge does not have to retranslate the UI. Add new wording to zh-Hans.tsv.
internal static class ZhHans
{
    private static readonly Dictionary<string, string> Exact = Load();
    private static readonly (Regex Pattern, string Replacement)[] Patterns =
    [
        (new(@"^(\d+) of (\d+) stages done$", RegexOptions.CultureInvariant), "$1 / $2 个阶段已完成"),
        (new(@"^All (\d+) stages done$", RegexOptions.CultureInvariant), "全部 $1 个阶段已完成"),
        (new(@"^(\d+) removal groups selected · (\d+) advanced actions$", RegexOptions.CultureInvariant), "已选择 $1 个移除组 · $2 个高级操作"),
        (new(@"^([\d,]+) components listed · ([\d,]+) advanced actions selected\.$", RegexOptions.CultureInvariant), "已列出 $1 个组件 · 已选择 $2 个高级操作"),
        (new(@"^Build mode: (.+)$", RegexOptions.CultureInvariant), "构建模式：$1"),
        (new(@"^Official Microsoft ISO · (.+)$", RegexOptions.CultureInvariant), "微软官方 ISO · $1"),
        (new(@"^Downloaded (.+)\. SHA-256: (.+)$", RegexOptions.CultureInvariant), "已下载 $1。SHA-256：$2"),
        (new(@"^(ProvisionedApp|SystemApp|Feature|Capability|Package|Service|ComponentPayload) · (.+)$", RegexOptions.CultureInvariant), "$1 · $2"),
        (new(@"^Failed: (.+) · (\d+) done( · \d+ (?:next|not run))?$", RegexOptions.CultureInvariant), "失败：$1 · $2 个已完成$3"),
        (new(@"^(\d+) failed · (\d+) done( · \d+ (?:next|not run))?$", RegexOptions.CultureInvariant), "$1 个失败 · $2 个已完成$3"),
        (new(@"^Cancelled · (\d+) done( · \d+ (?:next|not run))?$", RegexOptions.CultureInvariant), "已取消 · $1 个已完成$2"),
        (new(@"^In progress · (\d+) done( · \d+ (?:next|not run))?$", RegexOptions.CultureInvariant), "进行中 · $1 个已完成$2"),
        (new(@"^(\d+) done · (\d+) next$", RegexOptions.CultureInvariant), "$1 个已完成 · $2 个待处理"),
        (new(@"^(\d+) done · (\d+) not run$", RegexOptions.CultureInvariant), "$1 个已完成 · $2 个未运行"),
        (new(@"^(.*) · (\d+) not run$", RegexOptions.CultureInvariant), "$1 · $2 个未运行"),
        (new(@"^(.*) · (\d+) next$", RegexOptions.CultureInvariant), "$1 · $2 个待处理"),
    ];

    public static string T(string? text)
    {
        if (string.IsNullOrEmpty(text) || text.Contains('\\') || text.Contains("://", StringComparison.Ordinal)) return text ?? "";
        if (Exact.TryGetValue(text, out var translated)) return translated;
        foreach (var (pattern, replacement) in Patterns)
        {
            var match = pattern.Match(text);
            if (!match.Success) continue;
            var value = pattern.Replace(text, replacement);
            value = Regex.Replace(value, @" · (\d+) not run", " · $1 个未运行");
            value = Regex.Replace(value, @" · (\d+) next", " · $1 个待处理");
            return value.Replace("ProvisionedApp", T("ProvisionedApp"), StringComparison.Ordinal)
                .Replace("SystemApp", T("SystemApp"), StringComparison.Ordinal)
                .Replace("ComponentPayload", T("ComponentPayload"), StringComparison.Ordinal)
                .Replace("Capability", T("Capability"), StringComparison.Ordinal)
                .Replace("Feature", T("Feature"), StringComparison.Ordinal)
                .Replace("Package", T("Package"), StringComparison.Ordinal)
                .Replace("Service", T("Service"), StringComparison.Ordinal);
        }
        return text;
    }

    public static void Attach(Window window)
    {
        var timer = window.DispatcherQueue.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(400);
        timer.Tick += (_, _) =>
        {
            if (window.Content is UIElement content) Apply(content);
            var root = (window.Content as FrameworkElement)?.XamlRoot;
            if (root is null) return;
            foreach (var popup in VisualTreeHelper.GetOpenPopupsForXamlRoot(root))
                if (popup.Child is not null) Apply(popup.Child);
        };
        timer.Start();
        window.Closed += (_, _) => timer.Stop();
    }

    private static void Apply(DependencyObject node)
    {
        switch (node)
        {
            case TextBlock block:
                BindOrSet(block, TextBlock.TextProperty, block.Text, value => block.Text = value);
                break;
            case TextBox box:
                SetIfChanged(box.Header as string, value => box.Header = value);
                SetIfChanged(box.PlaceholderText, value => box.PlaceholderText = value);
                break;
            case ComboBox combo:
                SetIfChanged(combo.Header as string, value => combo.Header = value);
                SetIfChanged(combo.PlaceholderText, value => combo.PlaceholderText = value);
                break;
            case ToggleSwitch toggle:
                SetIfChanged(toggle.Header as string, value => toggle.Header = value);
                SetIfChanged(toggle.OnContent as string, value => toggle.OnContent = value);
                SetIfChanged(toggle.OffContent as string, value => toggle.OffContent = value);
                break;
            case ContentControl control:
                SetIfChanged(control.Content as string, value => control.Content = value);
                break;
            case InfoBar bar:
                SetIfChanged(bar.Title, value => bar.Title = value);
                SetIfChanged(bar.Message, value => bar.Message = value);
                break;
            case Expander expander:
                SetIfChanged(expander.Header as string, value => expander.Header = value);
                break;
            case ContentDialog dialog:
                SetIfChanged(dialog.Title as string, value => dialog.Title = value);
                SetIfChanged(dialog.PrimaryButtonText, value => dialog.PrimaryButtonText = value);
                SetIfChanged(dialog.SecondaryButtonText, value => dialog.SecondaryButtonText = value);
                SetIfChanged(dialog.CloseButtonText, value => dialog.CloseButtonText = value);
                break;
        }
        if (node is UIElement element)
        {
            SetIfChanged(AutomationProperties.GetName(element), value => AutomationProperties.SetName(element, value));
            SetIfChanged(AutomationProperties.GetHelpText(element), value => AutomationProperties.SetHelpText(element, value));
            SetIfChanged(AutomationProperties.GetFullDescription(element), value => AutomationProperties.SetFullDescription(element, value));
        }
        var count = VisualTreeHelper.GetChildrenCount(node);
        for (var i = 0; i < count; i++) Apply(VisualTreeHelper.GetChild(node, i));
    }

    // Bound text stays on the English view-model so filters and progress logic keep their keys.
    private static void BindOrSet(TextBlock block, DependencyProperty property, string current, Action<string> set)
    {
        var expression = block.GetBindingExpression(property);
        if (expression?.ParentBinding is { Converter: not ZhText } binding && binding.Path?.Path is { Length: > 0 } path)
        {
            block.SetBinding(property, new Binding
            {
                Path = new PropertyPath(path),
                Mode = binding.Mode,
                Converter = ZhText.Instance,
            });
            return;
        }
        if (expression is null) SetIfChanged(current, set);
    }

    private static void SetIfChanged(string? current, Action<string> set)
    {
        if (string.IsNullOrEmpty(current)) return;
        var translated = T(current);
        if (!string.Equals(translated, current, StringComparison.Ordinal)) set(translated);
    }

    private static Dictionary<string, string> Load()
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        using var stream = typeof(ZhHans).Assembly.GetManifestResourceStream("Tiny11Builder.App.Localization.zh-Hans.tsv");
        if (stream is null) return map;
        using var reader = new StreamReader(stream);
        while (reader.ReadLine() is { } line)
        {
            if (line.Length == 0 || line[0] == '#') continue;
            var split = line.IndexOf('\t');
            if (split <= 0 || split == line.Length - 1) continue;
            map[line[..split]] = line[(split + 1)..];
        }
        return map;
    }
}

internal sealed class ZhText : IValueConverter
{
    public static ZhText Instance { get; } = new();
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is string text ? ZhHans.T(text) : value;
    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}

# Tiny11 Builder 简体中文界面

这是 [ntdevlabs/tiny11builder](https://github.com/ntdevlabs/tiny11builder) 的 fork。英文原文仍是界面文案的来源，简体中文由 `src/Tiny11Builder.App/Localization/zh-Hans.tsv` 在运行时覆盖。

同步上游时拉取官方 `winui-preview`，再处理冲突。正常情况下冲突只会出现在这几处：

- `MainWindow.xaml.cs` 里调用 `ZhHans.Attach` 的那一行
- 两个筛选框改成按序号取值的代码
- `Tiny11Builder.App.csproj` 里嵌入 `zh-Hans.tsv` 的那一行

官方新增的英文句子不会自动变成中文。把原文和译文追加到 `zh-Hans.tsv` 即可，不要改 XAML 里的英文字符串。

PowerShell 脚本 `tiny11maker.ps1` 和 `tiny11Coremaker.ps1` 仍是英文。构建日志和 DISM 报错也保持英文。

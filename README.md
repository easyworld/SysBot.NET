# SysBot.NET
![License](https://img.shields.io/badge/License-AGPLv3-blue.svg)

## 支持用 Discord（Support Discord）

如果你需要在搭建自己的 SysBot.NET 实例时获得帮助，欢迎加入 Discord 获取支持！（请注意：有些非官方 Discord 可能冒充官方，请务必辨别）

[<img src="https://canary.discordapp.com/api/guilds/401014193211441153/widget.png?style=banner2">](https://discord.gg/tDMvSRv)

[sys-botbase](https://github.com/olliz0r/sys-botbase) 的客户端，用于对任天堂 Switch 主机进行远程控制与自动化操作。

## SysBot.Base：
- 可供各游戏专用项目构建与扩展的基础逻辑库。
- 包含用于与 sys-botbase 交互的同步与异步 Bot 连接类。

## SysBot.Tests：
- 单元测试，用于确保逻辑行为符合预期 :)

# 示例实现（Example Implementations）

开发这个项目的主要动力是为任天堂 Switch 的宝可梦游戏实现自动化机器人。本仓库提供了一个示例实现，用于展示该框架能够完成的一些有趣任务。更多已支持的宝可梦功能请参考 [Wiki](https://github.com/kwsch/SysBot.NET/wiki)。

## SysBot.Pokemon：
- 使用 SysBot.Base 的类库，包含与创建和运行《宝可梦 剑/盾》机器人相关的逻辑。

## SysBot.Pokemon.WinForms：
- 一个简单的 GUI 启动器，用于添加、启动与停止宝可梦机器人（如上所述）。
- 程序设置可在应用内配置，并保存为本地 JSON 文件。

## SysBot.Pokemon.Discord：
- 用于远程与 WinForms GUI 交互的 Discord 接口。
- 需要提供 Discord 登录 Token，以及允许与机器人交互的角色（Roles）。
- 提供用于管理与加入派送队列（distribution queue）的命令。

## SysBot.Pokemon.Twitch：
- Twitch.tv 接口，用于在派送开始时进行远程公告等操作。
- 需要提供 Twitch 登录 Token、用户名与要登录的频道。

## SysBot.Pokemon.YouTube：
- YouTube.com 接口，用于在派送开始时进行远程公告等操作。
- 需要提供 YouTube 登录用的 ClientID、ClientSecret 与 ChannelID。

通过 NuGet 依赖使用 [Discord.Net](https://github.com/discord-net/Discord.Net)、[TwitchLib](https://github.com/TwitchLib/TwitchLib) 和 [StreamingClientLibary](https://github.com/SaviorXTanren/StreamingClientLibrary)。

## SysBot.Pokemon.QQ：
- 支持 [ALM-Showdown-Sets](https://github.com/architdate/PKHeX-Plugins/wiki/ALM-Showdown-Sets)
- 支持 PK8 / PB8 / PA8 / PK9 文件上传

大部分代码基于 [SysBot.Pokemon.Twitch](https://github.com/kwsch/SysBot.NET/tree/master/SysBot.Pokemon.Twitch)。

通过 NuGet 依赖使用 [Mirai.Net](https://github.com/SinoAHpx/Mirai.Net)。

文档：[搭建指南](https://github.com/easyworld/SysBot.NET/tree/master/SysBot.Pokemon.QQ)、[命令指南](https://docs.qq.com/doc/DSVlldkxMSW92VXZF)

## SysBot.Pokemon.Dodo（已失效 / Dead）：
**注意：Dodo 平台已于 2025 年 6 月 15 日停止服务。本项目与 Dodo 平台相关的功能均已无法使用。此仓库现仅作为代码参考用途归档——所有机器人功能与命令均不再可用。**
- 支持 [ALM-Showdown-Sets](https://github.com/architdate/PKHeX-Plugins/wiki/ALM-Showdown-Sets)
- 支持 PK8 / PB8 / PA8 / PK9 / PA9 文件上传
- 支持将自定义中文内容翻译为 ALM-Showdown-Sets 格式

大部分代码基于 [SysBot.Pokemon.Twitch](https://github.com/kwsch/SysBot.NET/tree/master/SysBot.Pokemon.Twitch)。

通过 NuGet 依赖使用 [dodo-open-net](https://github.com/dodo-open/dodo-open-net)。

文档：[搭建指南](https://docs.qq.com/doc/DSVVZZk9saUNTeHNn)、[命令指南](https://docs.qq.com/doc/DSVlldkxMSW92VXZF)

## SysBot.Pokemon.Kook：
**SysBot.Pokemon.Kook** 与 [Kook](https://www.kookapp.cn/)（原开黑啦）平台完全兼容。

- 支持 [ALM-Showdown-Sets](https://github.com/architdate/PKHeX-Plugins/wiki/ALM-Showdown-Sets)
- 支持 PK8 / PB8 / PA8 / PK9 / PA9 文件上传
- 支持将[自定义中文](https://docs.qq.com/doc/DSVlldkxMSW92VXZF)翻译为 ALM-Showdown-Sets 格式
- 易于配置与扩展，适配 Kook 生态

文档：[搭建指南](https://docs.qq.com/doc/DSWpDQnBwSXRGSGR3)、[命令指南](https://docs.qq.com/doc/DSVlldkxMSW92VXZF)

## 其他依赖（Other Dependencies）
宝可梦相关的 API 逻辑由 [PKHeX](https://github.com/kwsch/PKHeX/) 提供，模板生成由 [Auto-Legality Mod](https://github.com/architdate/PKHeX-Plugins/) 提供。目前的模板生成使用 [@santacrab2](https://www.github.com/santacrab2) 的 [Auto-Legality Mod 分支（fork）](https://github.com/santacrab2/PKHeX-Plugins)。

# 许可证（License）
有关许可协议的详细信息，请参阅 `License.md`。

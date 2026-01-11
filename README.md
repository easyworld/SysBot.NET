# SysBot.NET
![License](https://img.shields.io/badge/License-AGPLv3-blue.svg)

## Support Discord:

For support on setting up your own instance of SysBot.NET, feel free to join the discord! (Beware of un-official discords who claim to be official)

[<img src="https://canary.discordapp.com/api/guilds/401014193211441153/widget.png?style=banner2">](https://discord.gg/tDMvSRv)

[sys-botbase](https://github.com/olliz0r/sys-botbase) client for remote control automation of Nintendo Switch consoles.

## SysBot.Base:
- Base logic library to be built upon in game-specific projects.
- Contains a synchronous and asynchronous Bot connection class to interact with sys-botbase.

## SysBot.Tests:
- Unit Tests for ensuring logic behaves as intended :)

# Example Implementations

The driving force to develop this project is automated bots for Nintendo Switch Pokémon games. An example implementation is provided in this repo to demonstrate interesting tasks this framework is capable of performing. Refer to the [Wiki](https://github.com/kwsch/SysBot.NET/wiki) for more details on the supported Pokémon features.

## SysBot.Pokemon:
- Class library using SysBot.Base to contain logic related to creating & running Sword/Shield bots.

## SysBot.Pokemon.WinForms:
- Simple GUI Launcher for adding, starting, and stopping Pokémon bots (as described above).
- Configuration of program settings is performed in-app and is saved as a local json file.

## SysBot.Pokemon.Discord:
- Discord interface for remotely interacting with the WinForms GUI.
- Provide a discord login token and the Roles that are allowed to interact with your bots.
- Commands are provided to manage & join the distribution queue.

## SysBot.Pokemon.Twitch:
- Twitch.tv interface for remotely announcing when the distribution starts.
- Provide a Twitch login token, username, and channel for login.

## SysBot.Pokemon.YouTube:
- YouTube.com interface for remotely announcing when the distribution starts.
- Provide a YouTube login ClientID, ClientSecret, and ChannelID for login.

Uses [Discord.Net](https://github.com/discord-net/Discord.Net) , [TwitchLib](https://github.com/TwitchLib/TwitchLib) and [StreamingClientLibary](https://github.com/SaviorXTanren/StreamingClientLibrary) as a dependency via Nuget.

## SysBot.Pokemon.QQ:
- Support [ALM-Showdown-Sets](https://github.com/architdate/PKHeX-Plugins/wiki/ALM-Showdown-Sets)
- Support PK8 PB8 PA8 PK9 file upload

Most codes are based on [SysBot.Pokemon.Twitch](https://github.com/kwsch/SysBot.NET/tree/master/SysBot.Pokemon.Twitch)

Uses [Mirai.Net](https://github.com/SinoAHpx/Mirai.Net) as a dependency via Nuget.

Document: [搭建指南](https://github.com/easyworld/SysBot.NET/tree/master/SysBot.Pokemon.QQ), [命令指南](https://docs.qq.com/doc/DSVlldkxMSW92VXZF)

## SysBot.Pokemon.Dodo (Dead):
**Notice: The Dodo platform was discontinued in June 15th 2025. All related features of this project are no longer functional. This repository is now archived for code reference only—none of the bot functionalities or commands are available anymore.**
- Support [ALM-Showdown-Sets](https://github.com/architdate/PKHeX-Plugins/wiki/ALM-Showdown-Sets)
- Support PK8 PB8 PA8 PK9 PA9 file upload
- Support Customized Chinese to ALM-Showdown-Sets translation

Most codes are based on [SysBot.Pokemon.Twitch](https://github.com/kwsch/SysBot.NET/tree/master/SysBot.Pokemon.Twitch)

Uses [dodo-open-net](https://github.com/dodo-open/dodo-open-net) as a dependency via Nuget.

Document: [搭建指南](https://docs.qq.com/doc/DSVVZZk9saUNTeHNn), [命令指南](https://docs.qq.com/doc/DSVlldkxMSW92VXZF)

## SysBot.Pokemon.Kook:
**SysBot.Pokemon.Kook** is fully compatible with the [Kook](https://www.kookapp.cn/) (formerly known as Kaiheila) platform.

- Support [ALM-Showdown-Sets](https://github.com/architdate/PKHeX-Plugins/wiki/ALM-Showdown-Sets)
- Support PK8 PB8 PA8 PK9 PA9 file upload
- Support [Customized Chinese](https://docs.qq.com/doc/DSVlldkxMSW92VXZF) to ALM-Showdown-Sets translation
- Easily configurable and extensible for the Kook ecosystem.

Document: [搭建指南](https://docs.qq.com/doc/DSWpDQnBwSXRGSGR3), [命令指南](https://docs.qq.com/doc/DSVlldkxMSW92VXZF)

## Other Dependencies
Pokémon API logic is provided by [PKHeX](https://github.com/kwsch/PKHeX/), and template generation is provided by [Auto-Legality Mod](https://github.com/architdate/PKHeX-Plugins/). Current template generation uses [@santacrab2](https://www.github.com/santacrab2)'s [Auto-Legality Mod fork](https://github.com/santacrab2/PKHeX-Plugins).

# License
Refer to the `License.md` for details regarding licensing.

import type {Game, Launcher} from "@/@types/models";
import {execute, executeWithReturn} from "@/libs/bridge";
import {LauncherType} from "@/@types/models";
import {getPrioLauncherType} from "@/libs/settings-manager";

export class LauncherController {

    public static launchGame(game: Game): Promise<boolean> {
        return new Promise(resolve => {
            executeWithReturn<boolean>("LaunchGame", game.appid).then(value => {
                resolve(value);
            });
        });
    }

    public static async findGamePath(game: Game): Promise<string|false> {
        return new Promise(resolve => {
            executeWithReturn<string|null>("FindGamePath", getPrioLauncherType(), game.appid, game.appname).then(value => {
                console.log(value);
                if (value === null) {
                    resolve(false);
                } else {
                    resolve(value);
                }
            });
        });
    }

    public static async getLaunchers(): Promise<Launcher[]> {
        return new Promise((resolve) => {
            executeWithReturn<Launcher[]|false>("GetLaunchers").then((launchers) => {
                let result: Launcher[] = [];
                if (launchers === false) {
                    result = [
                        {
                            type: LauncherType.Steam,
                            path: "<REPLACE>"
                        },
                        {
                            type: LauncherType.Epic,
                            path: "<REPLACE>"
                        }
                    ];
                    alert("Your OS is currently not supported for automatic detection of launchers. Please enter the paths manually.");
                } else {
                    result = launchers;
                }

                this.saveLaunchers(result).then(() => {
                    resolve(result);
                });
            });
        });
    }

    private static async saveLaunchers(launchers: Launcher[]) {
        await execute("SaveLaunchers", launchers);

        return launchers;
    }
}

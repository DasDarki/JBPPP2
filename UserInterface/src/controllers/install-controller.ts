import {execute, executeWithReturn, generateUuid, onEvent} from "@/libs/bridge";
import type {Game} from "@/@types/models";
import {useStore} from "@/stores/main";

export class InstallController {

    private static downloadProgresses: {[key: string]: (percent: number) => void} = {};
    private static downloadFinishes: {[key: string]: (file: string) => void} = {};
    private static installProgresses: {[key: string]: (action: "EXTRACT"|"MOVE"|"CLEAN", count: number, total: number) => void} = {};
    private static installFinishes: {[key: string]: () => void} = {};
    private static uninstallProgresses: {[key: string]: (action: "MOVE"|"ERR01"|"ERR02", count: number, total: number) => void} = {};
    private static uninstallFinishes: {[key: string]: () => void} = {};

    public static initialize(): void {
        onEvent("DownloadProgress", (id: string, percent: number) => {
            const progress = this.downloadProgresses[id];
            if (progress) {
                progress(percent);
            }
        });
        onEvent("DownloadFinish", (id: string, file: string) => {
            console.log("DownloadFinish", id, file, this.downloadFinishes);
            const finish = this.downloadFinishes[id];
            if (finish) {
                finish(file);
            }
        });
        onEvent("InstallProgress", (id: string, action: "EXTRACT"|"MOVE"|"CLEAN", count: number, total: number) => {
            const progress = this.installProgresses[id];
            if (progress) {
                progress(action, count, total);
            }
        });
        onEvent("InstallFinish", (id: string) => {
            const finish = this.installFinishes[id];
            if (finish) {
                finish();
            }
        });
        onEvent("UninstallProgress", (id: string, action: "MOVE"|"ERR01"|"ERR02", count: number, total: number) => {
            const progress = this.uninstallProgresses[id];
            if (progress) {
                progress(action, count, total);
            }
        });
        onEvent("UninstallFinish", (id: string) => {
            const finish = this.uninstallFinishes[id];
            if (finish) {
                finish();
            }
        });
    }

    public static findDownloadLink(game: Game): string {
        const {getCurrentLocation} = useStore();

        const location = getCurrentLocation();
        if (!location || location.patch[game.shortname] === undefined) {
            return "";
        }

        return location.patch[game.shortname];
    }

    public static checkVersion(gamePath: string, game: Game): Promise<boolean> {
        const {getCurrentLocation} = useStore();

        return new Promise((resolve) => {
            (async () => {
                const location = getCurrentLocation();
                if (!location || location.version[game.shortname] === undefined) {
                    resolve(false);
                    return;
                }

                const newConfig = await fetch(location.version[game.shortname]).then((res) => res.json());
                const newVersion = newConfig.buildVersion;

                const success = await executeWithReturn<boolean>("CheckVersion", gamePath, game.config, newVersion);

                resolve(success);
            })();
        });
    }

    public static download(url: string, progress: ((percent: number) => void)|null = null): Promise<string> {
        return new Promise((resolve) => {
            const id = generateUuid();
            if (progress) {
                this.downloadProgresses[id] = progress;
            }
            this.downloadFinishes[id] = (file: string) => {
                delete this.downloadProgresses[id];
                resolve(file);
            };
            execute("Download", id, url);
        });
    }

    public static install(file: string, dest: string, progress: ((action: "EXTRACT"|"MOVE"|"CLEAN", count: number, total: number) => void)|null = null): Promise<void> {
        return new Promise((resolve) => {
            const id = generateUuid();
            if (progress) {
                this.installProgresses[id] = progress;
            }
            this.installFinishes[id] = () => {
                delete this.installProgresses[id];
                resolve();
            };
            execute("Install", id, file, dest);
        });
    }

    public static uninstall(dest: string, progress: ((action: "MOVE"|"ERR01"|"ERR02", count: number, total: number) => void)|null = null): Promise<void> {
        return new Promise((resolve, reject) => {
            const id = generateUuid();
            if (progress) {
                this.uninstallProgresses[id] = (action, count, total) => {
                    if (action === "ERR01" || action === "ERR02") {
                        delete this.uninstallProgresses[id];
                        reject(new Error(action));
                    } else {
                        progress(action, count, total);
                    }
                };
            }
            this.uninstallFinishes[id] = () => {
                delete this.uninstallProgresses[id];
                resolve();
            };
            execute("Uninstall", id, dest);
        });
    }

    public static async isPatchInstalled(gameFolder: string): Promise<boolean> {
        return await executeWithReturn<boolean>("IsPatchInstalled", gameFolder);
    }
}

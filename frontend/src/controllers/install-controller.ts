import {execute, generateUuid, onEvent} from "@/libs/bridge";

export class InstallController {

    private static downloadProgresses: {[key: string]: (percent: number) => void} = {};
    private static downloadFinishes: {[key: string]: (file: string) => void} = {};
    private static installProgresses: {[key: string]: (action: "EXTRACT"|"MOVE"|"CLEAN", count: number, total: string) => void} = {};
    private static installFinishes: {[key: string]: () => void} = {};

    public static initialize(): void {
        onEvent("DownloadProgress", (id: string, percent: number) => {
            const progress = this.downloadProgresses[id];
            if (progress) {
                progress(percent);
            }
        });
        onEvent("DownloadFinish", (id: string, file: string) => {
            const finish = this.downloadFinishes[id];
            if (finish) {
                finish(file);
            }
        });
        onEvent("InstallProgress", (id: string, action: "EXTRACT"|"MOVE"|"CLEAN", count: number, total: string) => {
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
    }

    public static download(url: string, progress: ((percent: number) => void)|null = null): Promise<string> {
        return new Promise((resolve) => {
            const id = generateUuid();
            if (progress) {
                this.downloadProgresses[id] = progress;
            }
            this.downloadFinishes[id] = (file: string) => {
                resolve(file);
            };
            execute("Download", id, url);
        });
    }

    public static install(file: string, dest: string, progress: ((action: "EXTRACT"|"MOVE"|"CLEAN", count: number, total: string) => void)|null = null): Promise<void> {
        return new Promise((resolve) => {
            const id = generateUuid();
            if (progress) {
                this.installProgresses[id] = progress;
            }
            this.installFinishes[id] = () => {
                resolve();
            };
            execute("Install", id, file, dest);
        });
    }
}
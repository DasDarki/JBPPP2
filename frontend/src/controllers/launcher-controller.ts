import type {Launcher} from "@/@types/models";
import {execute} from "@/libs/bridge";

export class LauncherController {

    public static async getLaunchers(): Promise<Launcher[]> {
        console.log("getLaunchers");
        await this.getLauncherFolderBySelect("Select Steam folder");
        return [];


    }

    private static async saveLaunchers(launchers: Launcher[]) {
        await execute("SaveLaunchers", launchers);

        return launchers;
    }

    private static async getLauncherFolderBySelect(title: string): Promise<string|null> {
        const fileInput = document.createElement("input");
        fileInput.type = "file";
        fileInput.multiple = false;
        fileInput.style.display = "none";
        document.body.appendChild(fileInput);
        return new Promise<string|null>(resolve => {
            fileInput.onchange = () => {
                if (fileInput.files?.length === 1) {
                    // @ts-ignore
                    resolve(fileInput.files[0].path);
                } else {
                    resolve(null);
                }
                document.body.removeChild(fileInput);
            };
            fileInput.click();
        });
    }
}
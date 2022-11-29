import {AvailableLocales} from "@/i18n";

export type PrioLaunchers = "Steam" | "Epic Games";

function getSystemLocale() {
    const langCode = navigator.language;
    const lang = AvailableLocales.find(l => l.code === langCode || l.code.startsWith(langCode.split('-')[0].toLowerCase()));

    return lang ? lang.code : 'en';
}

export class SettingsManager {

    public static getLocale(): string {
        let locale = localStorage.getItem('settings:language');
        if (!locale) {
           locale = getSystemLocale();
           this.setLocale(locale!);
        }

        return locale!;
    }

    public static setLocale(lang: string) {
        localStorage.setItem('settings:language', lang);
    }

    public static getPrioLauncher(): PrioLaunchers {
        let launcher: PrioLaunchers = <PrioLaunchers>localStorage.getItem('settings:prioLauncher');
        if (!launcher) {
            launcher = "Steam";
            this.setPrioLauncher(launcher);
        }

        return launcher;
    }

    public static setPrioLauncher(launcher: PrioLaunchers) {
        localStorage.setItem('settings:prioLauncher', launcher);
    }
}

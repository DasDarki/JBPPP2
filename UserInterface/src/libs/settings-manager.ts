import {AvailableLocales} from "@/i18n";
import {LauncherType} from "@/@types/models";

export type PrioLaunchers = "Steam" | "Epic Games";

function getSystemLocale() {
    const langCode = navigator.language;
    const lang = AvailableLocales.find(l => l.code === langCode || l.code.startsWith(langCode.split('-')[0].toLowerCase()));

    return lang ? lang.code : 'en';
}

export function getSetLocale() {
    let locale = localStorage.getItem('settings:language');
    if (!locale) {
        locale = getSystemLocale();
        setSetLocale(locale!);
    }

    return locale!;
}

export function setSetLocale(lang: string) {
    localStorage.setItem('settings:language', lang);
}


export function getPrioLauncher(): PrioLaunchers {
    let launcher: PrioLaunchers = <PrioLaunchers>localStorage.getItem('settings:prioLauncher');
    if (!launcher) {
        launcher = "Steam";
        setPrioLauncher(launcher);
    }

    return launcher;
}

export function getPrioLauncherType(): LauncherType {
    let launcher = getPrioLauncher();
    if (launcher === "Steam") {
        return LauncherType.Steam;
    } else if (launcher === "Epic Games") {
        return LauncherType.Epic;
    }

    return LauncherType.Steam;
}

export function setPrioLauncher(launcher: PrioLaunchers) {
    localStorage.setItem('settings:prioLauncher', launcher);
}

export function getLocation(): string {
    return localStorage.getItem('settings:location') || "de";
}

export function setLocation(location: string) {
    localStorage.setItem('settings:location', location);
}

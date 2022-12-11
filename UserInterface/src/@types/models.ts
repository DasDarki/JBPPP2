export interface Game {
    title: string;
    shortname: string;
    icon: string;
    appid: number;
    appname: string;
    exe: string;
    folder: string;
    config: string;
}

export interface StatusLanguage {
    name: string;
    hasTextTranslation: boolean;
    hasVoiceTranslation: boolean;
    workInProgress: boolean;
    recommendation: string;
    description: string;
}

export interface Status {
    gameID: number;
    name: string;
    minPlayers: number;
    maxPlayers: number;
    audienceSupport: boolean;
    forLivestreams: boolean;
    description: string;
    languages: {[name: string]: StatusLanguage[]}[];
}

export interface Location {
    "country-file": string;
    version: {[key: string]: string};
    "patch-locale": string;
    patch: {[key: string]: string};
}

export interface StatusList {
    [name: string]: Status[];
}

export interface LocationList {
    [name: string]: Location;
}

export enum LauncherType {
    Steam = 0,
    Epic = 1
}

export interface Launcher {
    path: string;
    type: LauncherType;
}

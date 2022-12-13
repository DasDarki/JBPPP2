import {executeWithReturn} from "@/libs/bridge";
import type {Game} from "@/@types/models";

export class DataController {

    public static getGames(): Promise<Game[]> {
        return executeWithReturn("GetGames");
    }
}
import { defineStore } from 'pinia'
import type {Game, Location, LocationList} from "@/@types/models";
import {DataController} from "@/controllers/data-controller";
import {getLocation} from "@/libs/settings-manager";

export const useStore = defineStore('main', {
  state: () => ({
    locations: null as LocationList | null,
  }),
  actions: {
    setLocations(locations: LocationList) {
      this.locations = locations
    },
    getCurrentLocation(): Location|null {
      const {locations} = this;
      const location = getLocation();

      if (locations) {
        for (const name in locations) {
          if (locations.hasOwnProperty(name)) {
            if (locations[name]["country-file"] === location) {
              return locations[name];
            }
          }
        }
      }

      return null;
    }
  }
})

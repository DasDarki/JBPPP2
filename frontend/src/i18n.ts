import VueI18n from 'vue-i18n'
import {SettingsManager} from "@/libs/SettingsManager";
import en from "@/assets/locales/en.json";

export const AvailableLocales = [{name: "English", code: "en"}];

const i18n = VueI18n.createI18n({
  locale: SettingsManager.getLocale(),
  fallbackLocale: 'en',
  messages: {
    en
  }
});

export function getLocale() {
  return i18n.global.locale;
}

export function setLocale(lang: string) {
  if (!AvailableLocales.find(l => l.code === lang)) {
    throw new Error(`Locale ${lang} is not available`);
  }

  if (i18n.global.locale === lang) {
    return;
  }

  // @ts-ignore
  i18n.global.locale = lang;
  SettingsManager.setLocale(lang);
}

export default i18n;

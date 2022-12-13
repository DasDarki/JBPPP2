import { createI18n } from 'vue-i18n'
import en from "@/assets/locales/en.json";
import de from "@/assets/locales/de.json";
import {getSetLocale, setSetLocale} from "@/libs/settings-manager";

export const AvailableLocales = [{name: "English", code: "en"}, {name: "Deutsch", code: "de"}];

const i18n = createI18n({
  locale: getSetLocale(),
  fallbackLocale: 'en',
  messages: {
    en, de
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
  setSetLocale(lang);
}

export default i18n;

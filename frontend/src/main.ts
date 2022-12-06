import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'

import '@fortawesome/fontawesome-free/css/all.css';
import '@/styles/main.scss';
import i18n from "@/i18n";
import {InstallController} from "@/controllers/install-controller";

InstallController.initialize();

const app = createApp(App)

app.use(i18n)
app.use(createPinia())

app.mount('#app')
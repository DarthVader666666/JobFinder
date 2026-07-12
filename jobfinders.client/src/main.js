import "./assets/main.css";
import "primeicons/primeicons.css";
import Aura from "@primevue/themes/aura";

import PrimeVue from "primevue/config";
import { createApp } from "vue";
import App from "./App.vue";
import ToastService from "primevue/toastservice";
import store from "./vuex/store";

const app = createApp(App);

app
  .use(PrimeVue, {
    theme: {
      preset: Aura,
      options: {
        darkModeSelector: ".fake-dark-selector",
      },
    },
  })
  .use(ToastService)
  .use(store)
  .mount("#app");

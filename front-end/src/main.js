import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { Quasar, Cookies, Notify } from 'quasar'
import router from './router'
import quasarLang from 'quasar/lang/pt-BR'
import quasarIconSet from 'quasar/icon-set/fontawesome-v6'

// Import icon libraries
import '@quasar/extras/fontawesome-v6/fontawesome-v6.css'

// Import Quasar css
import 'quasar/src/css/index.sass'

// Assumes your root component is App.vue
// and placed in same folder as main.js
import App from './App.vue'

const pinia = createPinia();
const myApp = createApp(App)

myApp.use(Quasar, {
  plugins: { Cookies, Notify }, // import Quasar plugins and add here
  lang: quasarLang,
  iconSet: quasarIconSet,
})

myApp.use(pinia)
myApp.use(router)

// Assumes you have a <div id="app"></div> in your index.html
myApp.mount('#app')
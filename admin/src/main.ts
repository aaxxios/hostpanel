/** Application entry: creates the app and installs the store, router and i18n. */
import { createApp } from 'vue'
import { pinia } from '@/pinia'
import App from '@/App.vue'
import router from '@/router'
import { i18n } from '@/i18n'
import '@/assets/main.css'

// Nothing is restored from browser storage here, in either mode. The API writes the
// session token into an httpOnly cookie the browser attaches on its own, so the router
// guard's /auth/me call carries the session without this app knowing a credential exists.

const app = createApp(App)
app.use(pinia)
app.use(router)
app.use(i18n)
app.mount('#app')

import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import { quasar, transformAssetUrls } from '@quasar/vite-plugin'
import { VitePWA } from 'vite-plugin-pwa'

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), 'VITE_');

  return {
    plugins: [
      vue({
        template: { transformAssetUrls }
      }),

      // @quasar/plugin-vite options list:
      // https://github.com/quasarframework/quasar/blob/dev/vite-plugin/index.d.ts
      quasar({
        sassVariables: 'src/quasar-variables.scss'
      }),

      VitePWA({
        strategies: 'injectManifest',
        srcDir: 'src',
        filename: 'firebase-messaging-sw.js',
        registerType: 'autoUpdate',
        injectRegister: 'auto',
        includeAssets: ['vite.svg', 'avatars/**/*'],
        manifest: {
          name: 'WorshipHub',
          short_name: 'WorshipHub',
          description: 'Gestão de escalas de louvor para igrejas',
          theme_color: '#434d61',
          background_color: '#e7e7e7',
          display: 'standalone',
          orientation: 'portrait',
          scope: '/',
          start_url: '/',
          lang: 'pt-BR',
          icons: [
            { src: 'pwa-192x192.png', sizes: '192x192', type: 'image/png' },
            { src: 'pwa-512x512.png', sizes: '512x512', type: 'image/png' },
            { src: 'pwa-512x512.png', sizes: '512x512', type: 'image/png', purpose: 'maskable' }
          ]
        },
        injectManifest: {
          globPatterns: ['**/*.{js,css,html,ico,png,svg,woff2}'],
        },
        devOptions: {
          enabled: true,
          type: 'module',
          navigateFallback: 'index.html'
        }
      })
    ]
  }
})
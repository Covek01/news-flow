import type { Config } from 'tailwindcss'

export default {
  content: [
    "./src/**/*.{js,jsx,ts,tsx}",
  ],
  theme: {
    extend: {
      transparent: 'transparent',
      current: 'currentColor',
      lightBlue: '#4dabf5',
      blue: '#2196f3',
      darkBlue: '#1769aa',
      veryLightBlue: '#90caf9'
    },
  },
  plugins: [],
} satisfies Config


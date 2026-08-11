/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,jsx}'],
  theme: {
    extend: {
      colors: {
        cream: '#FBF3EA',
        card: '#FFFFFF',
        blush: '#F3DED2',
        blushdark: '#EFE5DC',
        espresso: '#3E2723',
        espressohover: '#54332E',
        clay: '#C1694F',
        claydark: '#8C4A36',
        gold: '#D4A857',
        danger: '#B23A2E',
        ink: '#3B2E2A',
        muted: '#8A7266',
      },
      fontFamily: {
        display: ['"Fraunces"', 'serif'],
        body: ['"Work Sans"', 'sans-serif'],
      },
      boxShadow: {
        soft: '0 14px 34px -12px rgba(59, 46, 42, 0.18)',
        card: '0 10px 24px -10px rgba(59, 46, 42, 0.14)',
      },
      backgroundImage: {
        glow: 'radial-gradient(60% 60% at 50% 40%, rgba(212,168,87,0.35) 0%, rgba(193,105,79,0.18) 45%, rgba(251,243,234,0) 75%)',
      },
    },
  },
  plugins: [],
}

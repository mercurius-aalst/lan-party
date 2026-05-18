/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Components/**/*.{razor,cshtml,html}",
    "./Extensions/**/*.cs",
    "./Services/**/*.cs",
    "./wwwroot/**/*.{html,js}"
  ],
  corePlugins: {
    preflight: false
  },
  theme: {
    screens: {
      sm: "640px",
      md: "860px",
      lg: "960px",
      xl: "1100px",
      "2xl": "1280px"
    },
    extend: {}
  }
};

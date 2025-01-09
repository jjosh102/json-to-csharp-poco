module.exports = {
    content: ["./**/*.{razor,html,cshtml}"],
    theme: {
        extend: {
            keyframes: {
                bounceFadeIn: {
                    '0%': { opacity: '0', transform: 'translateY(10px)' },
                    '50%': { opacity: '1', transform: 'translateY(0)' },
                    '100%': { opacity: '0', transform: 'translateY(-10px)' },
                },
            },
            animation: {
                'bounce-fade-in': 'bounceFadeIn 1.5s ease-in-out infinite',
            },
        },
    },
    darkMode: 'class',
    plugins: [],
}

window.initSwiper = (id) => {
    let el = document.getElementById(id);
    if (!el) return;
    let posterSize = 100;
    let margin = 4;

    var swiper = new Swiper(el, {
        slidesPerView: 'auto',
        spaceBetween: 4,
        breakpointsBase: 'container',
        navigation:
        {
            nextEl: ".swiper-button-next",
            prevEl: ".swiper-button-prev",
        },
        pagination:
        {
            el: ".swiper-pagination",
            clickable: true,
        },
        breakpoints: {
            [250 - margin]: { slidesPerView: Math.floor(250 / posterSize) },
            [300 - margin]: { slidesPerView: Math.floor(300 / posterSize) },
            [350 - margin]: { slidesPerView: Math.floor(350 / posterSize) },
            [400 - margin]: { slidesPerView: Math.floor(400 / posterSize) },
            [500 - margin]: { slidesPerView: Math.floor(500 / posterSize) },
            [600 - margin]: { slidesPerView: Math.floor(600 / posterSize) },
            [700 - margin]: { slidesPerView: Math.floor(700 / posterSize) },
            [800 - margin]: { slidesPerView: Math.floor(800 / posterSize) },
            [1000 - margin]: { slidesPerView: Math.floor(1000 / posterSize) },
            [1200 - margin]: { slidesPerView: Math.floor(1200 / posterSize) },
            [1400 - margin]: { slidesPerView: Math.floor(1400 / posterSize) },
            [1600 - margin]: { slidesPerView: Math.floor(1600 / posterSize) },
            [2000 - margin]: { slidesPerView: Math.floor(2000 / posterSize) },
        },
    });
};

window.initCalendar = (id) => {
    let el = document.getElementById(id);
    if (!el) return;

    const progressCircle = document.querySelector(".autoplay-progress svg");
    const progressContent = document.querySelector(".autoplay-progress span");

    var swiper = new Swiper(el, {
        centeredSlides: true,
        lazy: true,
        autoplay: {
            delay: 2500,
            disableOnInteraction: false,
        },
        navigation:
        {
            nextEl: ".swiper-button-next",
            prevEl: ".swiper-button-prev",
        },
        pagination:
        {
            el: ".swiper-pagination",
            clickable: true,
        },
        on: {
            autoplayTimeLeft(s, time, progress) {
                progressCircle.style.setProperty("--progress", 1 - progress);
                progressContent.textContent = `${Math.ceil(time / 1000)}s`;
            }
        }
    });
};

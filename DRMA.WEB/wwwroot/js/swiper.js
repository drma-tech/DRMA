//lists
window.initSwiper = (id, size) => {
    if (typeof Swiper === "undefined") {
        setTimeout(() => window.initSwiper(id, size), 200);
        return;
    }

    let mobile = window.innerHeight < 600 || window.innerWidth < 600;

    const el = document.getElementById(id);
    if (!el) return;
    const posterSize = size ?? (mobile ? 98 : 128);
    const margin = 8;

    var swiper = new Swiper(el, {
        slidesPerView: "auto",
        spaceBetween: mobile ? 4 : 8,
        breakpointsBase: "container",
        navigation: {
            nextEl: ".swiper-button-next",
            prevEl: ".swiper-button-prev",
        },
        pagination: {
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

//news
window.initCalendar = (id) => {
    if (typeof Swiper === "undefined") {
        setTimeout(() => window.initCalendar(id), 200);
        return;
    }

    const el = document.getElementById(id);
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
        navigation: {
            nextEl: ".swiper-button-next",
            prevEl: ".swiper-button-prev",
        },
        pagination: {
            el: ".swiper-pagination",
            clickable: true,
        },
        on: {
            autoplayTimeLeft(s, time, progress) {
                progressCircle.style.setProperty("--progress", 1 - progress);
                progressContent.textContent = `${Math.ceil(time / 1000)}s`;
            },
        },
    });
};

//trailers
window.initGrid = (id) => {
    if (typeof Swiper === "undefined") {
        setTimeout(() => window.initGrid(id), 200);
        return;
    }

    let mobile = window.innerHeight < 600 || window.innerWidth < 600;

    const el = document.getElementById(id);
    if (!el) return;
    const posterSize = 150;
    const margin = mobile ? 4 : 8;

    if (el.swiper && typeof el.swiper.destroy === "function") {
        el.swiper.destroy(true, true);
    }

    var swiper = new Swiper(el, {
        slidesPerView: "auto",
        spaceBetween: 4,
        breakpointsBase: "container",
        grid: {
            rows: 2,
        },
        navigation: {
            nextEl: ".swiper-button-next",
            prevEl: ".swiper-button-prev",
        },
        pagination: {
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
        on: {
            init: function () {
                // Function to set the height of the grid based on the tallest slide
                function setGridHeight() {
                    const slides = el.querySelectorAll(".swiper-slide");
                    if (slides.length > 0) {
                        let maxHeight = 0;
                        slides.forEach((slide) => {
                            slide.style.height = "auto";
                            const h = slide.offsetHeight;
                            if (h > maxHeight) maxHeight = h;
                        });
                        el.style.height = maxHeight * 2 + 8 + "px";
                    }
                }

                // Set the height of the grid after all images are loaded
                const images = el.querySelectorAll("img");
                let loaded = 0;
                if (images.length === 0) {
                    setGridHeight();
                } else {
                    images.forEach((img) => {
                        if (img.complete) {
                            loaded++;
                        } else {
                            img.addEventListener("load", () => {
                                loaded++;
                                if (loaded === images.length) setGridHeight();
                            });
                            img.addEventListener("error", () => {
                                loaded++;
                                if (loaded === images.length) setGridHeight();
                            });
                        }
                    });
                    if (loaded === images.length) setGridHeight();
                }
            },
        },
    });
};

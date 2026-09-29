window.journeyLanding = {
    storyScrollHandler: null,
    storyResizeHandler: null,

    initialize: function () {
        this.cleanup();

        this.initializeRevealAnimations();
        this.initializeStoryPath();
        this.initializeStoryMoments();
    },

    cleanup: function () {
        if (this.storyScrollHandler) {
            window.removeEventListener(
                "scroll",
                this.storyScrollHandler
            );

            this.storyScrollHandler = null;
        }

        if (this.storyResizeHandler) {
            window.removeEventListener(
                "resize",
                this.storyResizeHandler
            );

            this.storyResizeHandler = null;
        }
    },

    initializeRevealAnimations: function () {

        const elements =
            document.querySelectorAll("[data-reveal]");

        if (!elements.length) {
            return;
        }

        const prefersReducedMotion =
            window.matchMedia(
                "(prefers-reduced-motion: reduce)"
            ).matches;

        if (prefersReducedMotion) {
            elements.forEach(element => {
                element.classList.add("is-visible");
            });

            return;
        }

        const observer =
            new IntersectionObserver(
                entries => {

                    entries.forEach(entry => {

                        if (!entry.isIntersecting) {
                            return;
                        }

                        entry.target
                            .classList
                            .add("is-visible");

                        observer.unobserve(
                            entry.target
                        );
                    });
                },
                {
                    threshold: 0.18,
                    rootMargin:
                        "0px 0px -60px 0px"
                }
            );

        elements.forEach(element => {

            const rect =
                element.getBoundingClientRect();

            /*
             * If the element is already visible when the
             * user returns to the page, show it immediately.
             */
            if (
                rect.top < window.innerHeight &&
                rect.bottom > 0
            ) {
                element.classList.add(
                    "is-visible"
                );
            }
            else {
                observer.observe(element);
            }
        });
    },

    initializeStoryPath: function () {

        const story =
            document.getElementById(
                "journeyStory"
            );

        const svg =
            document.getElementById(
                "storyRouteSvg"
            );

        const path =
            document.getElementById(
                "storyRouteProgress"
            );

        const walker =
            document.getElementById(
                "storyRouteWalker"
            );

        if (!story || !svg || !path || !walker) {
            return;
        }

        const prefersReducedMotion =
            window.matchMedia(
                "(prefers-reduced-motion: reduce)"
            ).matches;

        const totalLength =
            path.getTotalLength();

        path.style.strokeDasharray =
            totalLength;

        path.style.strokeDashoffset =
            totalLength;

        if (prefersReducedMotion) {
            path.style.strokeDashoffset = 0;
            walker.style.display = "none";
            return;
        }

        let ticking = false;

        const updateStoryProgress = () => {

            ticking = false;

            /*
             * LandingPage may have been removed from
             * the DOM after Blazor navigation.
             */
            if (!document.body.contains(story)) {
                return;
            }

            const storyRect =
                story.getBoundingClientRect();

            const viewportHeight =
                window.innerHeight;

            const start =
                viewportHeight * 0.75;

            const end =
                -storyRect.height +
                viewportHeight * 0.25;

            let progress =
                (start - storyRect.top) /
                (start - end);

            progress =
                Math.max(
                    0,
                    Math.min(1, progress)
                );

            path.style.strokeDashoffset =
                totalLength *
                (1 - progress);

            const point =
                path.getPointAtLength(
                    totalLength * progress
                );

            const svgRect =
                svg.getBoundingClientRect();

            const viewBox =
                svg.viewBox.baseVal;

            const scaleX =
                svgRect.width /
                viewBox.width;

            const scaleY =
                svgRect.height /
                viewBox.height;

            const x =
                (point.x - viewBox.x) *
                scaleX;

            const y =
                (point.y - viewBox.y) *
                scaleY;

            walker.style.transform =
                `translate(
                    ${svgRect.left - storyRect.left + x - 23}px,
                    ${svgRect.top - storyRect.top + y - 23}px
                )`;
        };

        const requestUpdate = () => {

            if (ticking) {
                return;
            }

            ticking = true;

            requestAnimationFrame(
                updateStoryProgress
            );
        };

        this.storyScrollHandler =
            requestUpdate;

        this.storyResizeHandler =
            requestUpdate;

        window.addEventListener(
            "scroll",
            this.storyScrollHandler,
            { passive: true }
        );

        window.addEventListener(
            "resize",
            this.storyResizeHandler
        );

        updateStoryProgress();
    },

    initializeStoryMoments: function () {

        const sections =
            document.querySelectorAll(
                "[data-story-stage]"
            );

        const walker =
            document.getElementById(
                "storyRouteWalker"
            );

        const walkerIcon =
            document.getElementById(
                "storyWalkerIcon"
            );

        const rebuildingPath =
            document.querySelector(
                ".rebuilding-path"
            );

        if (
            !sections.length ||
            !walker ||
            !walkerIcon
        ) {
            return;
        }


        const setStage = stage => {

            walker.classList.remove(
                "running",
                "walking",
                "paused",
                "rebuilding"
            );


            switch (stage) {

                case "slow":

                    walker.classList.add(
                        "running"
                    );

                    walkerIcon.className =
                        "fa-solid fa-person-running";

                    if (rebuildingPath) {
                        rebuildingPath.classList.remove(
                            "story-active"
                        );
                    }

                    /*
                    * Slow down after the visitor reaches
                    * this section.
                    */
                    window.setTimeout(() => {

                        if (
                            walker.classList.contains(
                                "running"
                            )
                        ) {
                            walker.classList.remove(
                                "running"
                            );

                            walker.classList.add(
                                "walking"
                            );

                            walkerIcon.className =
                                "fa-solid fa-person-walking";
                        }

                    }, 1700);

                    break;


                case "reflect":

                    walker.classList.add(
                        "paused"
                    );

                    walkerIcon.className =
                        "fa-solid fa-person-walking";

                    if (rebuildingPath) {
                        rebuildingPath.classList.remove(
                            "story-active"
                        );
                    }

                    break;


                case "understand":

                    walker.classList.add(
                        "walking"
                    );

                    walkerIcon.className =
                        "fa-solid fa-person-walking";

                    if (rebuildingPath) {
                        rebuildingPath.classList.remove(
                            "story-active"
                        );
                    }

                    break;


                case "recreate":

                    walker.classList.add(
                        "rebuilding"
                    );

                    walkerIcon.className =
                        "fa-solid fa-person-walking";

                    if (rebuildingPath) {
                        rebuildingPath.classList.add(
                            "story-active"
                        );
                    }

                    break;
            }
        };


        const observer =
            new IntersectionObserver(
                entries => {

                    entries.forEach(entry => {

                        if (!entry.isIntersecting) {
                            return;
                        }

                        const stage =
                            entry.target.dataset.storyStage;

                        setStage(stage);
                    });
                },
                {
                    threshold: 0.45,

                    rootMargin:
                        "-20% 0px -35% 0px"
                }
            );


        sections.forEach(section => {
            observer.observe(section);
        });
    },
};
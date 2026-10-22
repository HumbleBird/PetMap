let sectionObserver;

export function observeLandingSections() {
    const sections = document.querySelectorAll(
        ".landing-page .feature-card, .landing-page .guide-section, .landing-page .landing-footer"
    );

    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches || !("IntersectionObserver" in window)) {
        sections.forEach(section => section.classList.add("is-visible"));
        return;
    }

    document.querySelector(".landing-page")?.classList.add("landing-effects-ready");
    sectionObserver = new IntersectionObserver(entries => {
        for (const entry of entries) {
            if (!entry.isIntersecting) continue;
            entry.target.classList.add("is-visible");
            sectionObserver.unobserve(entry.target);
        }
    }, { threshold: 0.12, rootMargin: "0px 0px -6% 0px" });

    sections.forEach(section => sectionObserver.observe(section));
}

export function stopLandingSectionObservation() {
    sectionObserver?.disconnect();
    sectionObserver = undefined;
    document.querySelector(".landing-page")?.classList.remove("landing-effects-ready");
}

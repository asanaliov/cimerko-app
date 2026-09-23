// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.querySelectorAll("[data-listing-type-form]").forEach(form => {
    const typeSelect = form.querySelector("[data-listing-type]");
    const typeHelp = form.querySelector("[data-listing-type-help]");
    const roommatesField = form.querySelector("[data-roommates-needed-field]");
    const roommatesInput = form.querySelector("[data-roommates-needed]");
    const bedroomsField = form.querySelector("[data-bedrooms-field]");
    const bedroomsInput = form.querySelector("[data-bedrooms]");
    const studioToggle = form.querySelector("[data-studio-toggle]");
    const factsFields = form.querySelector("[data-listing-facts-fields]");
    const preferencesSection = form.querySelector("[data-listing-preferences-section]");
    const rentalPreferences = form.querySelector("[data-rental-preferences]");
    const roommatePreferences = form.querySelector("[data-roommate-preferences]");

    if (!typeSelect ||
        !typeHelp ||
        !roommatesField ||
        !roommatesInput ||
        !bedroomsField ||
        !bedroomsInput ||
        !studioToggle ||
        !preferencesSection ||
        !rentalPreferences ||
        !roommatePreferences) {
        return;
    }

    const placeForRentValue = "1";
    const lookingForRoommateValue = "2";

    const updateStudioFields = (hasBedroomDetails, isPlaceForRent) => {
        studioToggle.disabled = !hasBedroomDetails;
        bedroomsInput.disabled = !hasBedroomDetails || studioToggle.checked;
        bedroomsInput.required = isPlaceForRent && !studioToggle.checked;

        if (hasBedroomDetails && studioToggle.checked) {
            bedroomsInput.value = "0";
        }
    };

    const updateListingTypeFields = () => {
        const isPlaceForRent = typeSelect.value === placeForRentValue;
        const isLookingForRoommate = typeSelect.value === lookingForRoommateValue;
        const hasBedroomDetails = isPlaceForRent || isLookingForRoommate;

        typeHelp.textContent = isPlaceForRent
            ? "You have a room, apartment, or house to rent."
            : isLookingForRoommate
                ? "You have or know a place and need one or more roommates."
                : "Choose whether you are listing a place for rent or looking for a roommate.";

        roommatesField.hidden = !isLookingForRoommate;
        roommatesInput.disabled = !isLookingForRoommate;
        roommatesInput.required = isLookingForRoommate;
        bedroomsField.hidden = !hasBedroomDetails;
        updateStudioFields(hasBedroomDetails, isPlaceForRent);
        preferencesSection.hidden = !isPlaceForRent && !isLookingForRoommate;
        rentalPreferences.hidden = !isPlaceForRent;
        roommatePreferences.hidden = !isLookingForRoommate;
        rentalPreferences.querySelectorAll("select, input").forEach(input => {
            input.disabled = !isPlaceForRent;
        });
        roommatePreferences.querySelectorAll("select, input").forEach(input => {
            input.disabled = !isLookingForRoommate;
        });
        factsFields?.classList.toggle("is-place-for-rent", isPlaceForRent);
    };

    typeSelect.addEventListener("change", updateListingTypeFields);
    studioToggle.addEventListener("change", updateListingTypeFields);
    updateListingTypeFields();
});

document.querySelectorAll("[data-home-featured-rotator]").forEach(rotator => {
    const slides = [...rotator.querySelectorAll("[data-home-featured-slide]")];

    if (slides.length < 2) {
        return;
    }

    let activeIndex = slides.findIndex(slide => !slide.hidden);

    if (activeIndex < 0) {
        activeIndex = 0;
    }

    const showSlide = index => {
        activeIndex = (index + slides.length) % slides.length;

        slides.forEach((slide, slideIndex) => {
            const isActive = slideIndex === activeIndex;

            slide.hidden = !isActive;
            slide.classList.toggle("is-active", isActive);
        });
    };

    window.setInterval(() => {
        if (!document.hidden) {
            showSlide(activeIndex + 1);
        }
    }, 7000);
});

document.querySelectorAll("[data-photo-gallery]").forEach(gallery => {
    const image = gallery.querySelector("[data-gallery-image]");
    const currentPhoto = gallery.querySelector("[data-gallery-current]");
    const photos = [...gallery.querySelectorAll("[data-gallery-photo]")];
    const openButtons = [
        ...document.querySelectorAll(`[data-gallery-open="${gallery.id}"]`)
    ];
    const closeButton = gallery.querySelector("[data-gallery-close]");
    const previousButton = gallery.querySelector("[data-gallery-previous]");
    const nextButton = gallery.querySelector("[data-gallery-next]");
    let activeIndex = 0;
    let opener = null;

    if (!image || !currentPhoto || photos.length === 0 || !closeButton) {
        return;
    }

    const showPhoto = index => {
        activeIndex = (index + photos.length) % photos.length;
        const activePhoto = photos[activeIndex];

        image.src = activePhoto.dataset.imageUrl;
        image.alt = activePhoto.dataset.imageAlt;
        currentPhoto.textContent = activeIndex + 1;

        photos.forEach((photo, photoIndex) => {
            const isActive = photoIndex === activeIndex;
            photo.classList.toggle("is-active", isActive);
            photo.setAttribute("aria-pressed", isActive.toString());
        });

        activePhoto.scrollIntoView({
            behavior: "smooth",
            block: "nearest",
            inline: "center"
        });
    };

    const openGallery = event => {
        opener = event.currentTarget;
        gallery.showModal();
        showPhoto(Number(opener.dataset.galleryIndex) || 0);
        closeButton.focus();
    };

    openButtons.forEach(button => {
        button.addEventListener("click", openGallery);
    });

    photos.forEach((photo, photoIndex) => {
        photo.addEventListener("click", () => showPhoto(photoIndex));
    });

    previousButton?.addEventListener("click", () => showPhoto(activeIndex - 1));
    nextButton?.addEventListener("click", () => showPhoto(activeIndex + 1));
    closeButton.addEventListener("click", () => gallery.close());

    gallery.addEventListener("click", event => {
        if (event.target === gallery) {
            gallery.close();
        }
    });

    gallery.addEventListener("keydown", event => {
        if (event.key === "ArrowLeft") {
            event.preventDefault();
            showPhoto(activeIndex - 1);
        }

        if (event.key === "ArrowRight") {
            event.preventDefault();
            showPhoto(activeIndex + 1);
        }
    });

    gallery.addEventListener("close", () => {
        opener?.focus();
    });
});

(() => {
    const revealTargets = [
        ".home-section-heading",
        ".home-listing-grid",
        ".home-steps li",
        ".home-final-cta",
        ".listing-results-heading",
        ".listing-card",
        ".roommate-card",
        ".listing-detail-section",
        ".listing-detail-aside",
        ".profile-completion",
        ".profile-content-card",
        ".profile-listings-section",
        ".profile-reviews-section",
        ".profile-listing-card",
        ".profile-review-card",
        ".profile-compatibility-card",
        ".profile-review-summary-card",
        ".profile-review-form-card",
        ".request-card",
        ".notification-item"
    ].join(",");

    const elements = [...document.querySelectorAll(revealTargets)];

    if (elements.length === 0 ||
        !("IntersectionObserver" in window) ||
        window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
        return;
    }

    document.documentElement.classList.add("reveal-ready");

    const observer = new IntersectionObserver(entries => {
        entries
            .filter(entry => entry.isIntersecting)
            .forEach((entry, batchIndex) => {
                entry.target.style.setProperty("--reveal-delay", `${Math.min(batchIndex, 5) * 80}ms`);
                entry.target.classList.add("is-revealed");
                observer.unobserve(entry.target);
            });
    }, { rootMargin: "0px 0px -8% 0px", threshold: 0.12 });

    elements.forEach(element => {
        element.setAttribute("data-reveal", "");
        observer.observe(element);
    });
})();

document.querySelectorAll("[data-listing-filters]").forEach(filters => {
    filters.querySelector("[data-filters-close]")?.addEventListener("click", () => filters.open = false);

    document.addEventListener("click", event => {
        if (filters.open && !filters.contains(event.target)) {
            filters.open = false;
        }
    });

    document.addEventListener("keydown", event => {
        if (event.key === "Escape" && filters.open) {
            filters.open = false;
            filters.querySelector("summary").focus();
        }
    });
});

document.querySelectorAll("[data-auto-submit]").forEach(input => {
    input.addEventListener("change", () => input.form?.submit());
});

document.querySelectorAll(".listing-card-save-form").forEach(form => {
    form.addEventListener("submit", async event => {
        event.preventDefault();

        const button = form.querySelector(".listing-card-save-btn");
        const listingTitle = form.dataset.listingTitle;
        button.disabled = true;

        try {
            const response = await fetch(form.action, {
                method: "POST",
                body: new FormData(form),
                headers: {
                    "Accept": "application/json",
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            if (!response.ok) {
                throw new Error(`Save request failed with status ${response.status}.`);
            }

            const result = await response.json();
            const isSaved = result.isSaved === true;

            form.action = isSaved ? form.dataset.removeUrl : form.dataset.saveUrl;
            button.classList.toggle("is-saved", isSaved);
            button.title = isSaved ? "Remove from saved listings" : "Save listing";
            button.setAttribute("aria-label", isSaved
                ? `Remove ${listingTitle} from saved listings`
                : `Save ${listingTitle}`);
        }
        catch (error) {
            console.error(error);
        }
        finally {
            button.disabled = false;
        }
    });
});

document.querySelectorAll("[data-share-listing]").forEach(button => {
    const label = button.querySelector("[data-share-label]");

    button.addEventListener("click", async () => {
        const url = window.location.href;

        try {
            if (navigator.share) {
                await navigator.share({ title: document.title, url });
                return;
            }

            await navigator.clipboard.writeText(url);
            label.textContent = "Link copied";
            setTimeout(() => label.textContent = "Share", 2000);
        }
        catch (error) {
            if (error.name !== "AbortError") {
                window.prompt("Copy this listing link:", url);
            }
        }
    });
});

document.querySelectorAll(".site-header").forEach(header => {
    const update = () => header.classList.toggle("is-scrolled", window.scrollY > 8);
    update();
    window.addEventListener("scroll", update, { passive: true });
});

document.querySelectorAll("[data-home-tabs]").forEach(tabList => {
    const tabs = [...tabList.querySelectorAll("[role='tab']")];
    const grid = document.querySelector("[data-home-grid]");
    const empty = document.querySelector("[data-home-filter-empty]");
    const viewAll = document.querySelector("[data-home-view-all]");
    const maxCards = 6;

    if (!grid) {
        return;
    }

    const cards = [...grid.querySelectorAll(".home-listing-card")];
    const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    const matches = (card, filter) =>
        filter === "all" ||
        (filter === "rent" && card.dataset.type === "rent") ||
        (filter === "roommate" && card.dataset.type === "roommate") ||
        (filter === "studio" && card.dataset.studio === "true") ||
        (filter === "available" && card.dataset.available === "true");
    let switching = null;

    const showCards = filter => {
        const visible = cards.filter(card => matches(card, filter)).slice(0, maxCards);
        cards.forEach(card => {
            card.hidden = !visible.includes(card);
            card.classList.remove("is-leaving", "is-entering");
        });
        visible.forEach((card, index) => {
            card.style.setProperty("--i", index);
            if (!reduceMotion) {
                card.classList.add("is-entering");
            }
        });
        grid.hidden = visible.length === 0;
        if (empty) {
            empty.hidden = visible.length > 0;
        }
    };

    const select = tab => {
        tabs.forEach(item => {
            const isActive = item === tab;
            item.classList.toggle("is-active", isActive);
            item.setAttribute("aria-selected", String(isActive));
            item.tabIndex = isActive ? 0 : -1;
        });

        tab.scrollIntoView({ behavior: "smooth", block: "nearest", inline: "nearest" });

        if (viewAll) {
            viewAll.href = tab.dataset.href;
            viewAll.querySelector("span").textContent = tab.dataset.label;
        }

        clearTimeout(switching);
        if (reduceMotion) {
            showCards(tab.dataset.filter);
            return;
        }

        // Fade the current cards out, swap them, then let the grid ease to its new height.
        const startHeight = grid.hidden ? 0 : grid.offsetHeight;
        cards.filter(card => !card.hidden).forEach(card => card.classList.add("is-leaving"));
        switching = setTimeout(() => {
            showCards(tab.dataset.filter);
            if (grid.hidden) {
                return;
            }

            const endHeight = grid.offsetHeight;
            grid.style.height = `${startHeight}px`;
            grid.classList.add("is-resizing");
            requestAnimationFrame(() => {
                grid.style.height = `${endHeight}px`;
            });
            switching = setTimeout(() => {
                grid.classList.remove("is-resizing");
                grid.style.height = "";
            }, 280);
        }, 150);
    };

    tabs.forEach((tab, index) => {
        tab.tabIndex = tab.classList.contains("is-active") ? 0 : -1;
        tab.addEventListener("click", () => {
            if (!tab.classList.contains("is-active")) {
                select(tab);
            }
        });
        tab.addEventListener("keydown", event => {
            if (event.key !== "ArrowRight" && event.key !== "ArrowLeft") {
                return;
            }

            event.preventDefault();
            const next = tabs[(index + (event.key === "ArrowRight" ? 1 : -1) + tabs.length) % tabs.length];
            next.focus();
            select(next);
        });
    });
});

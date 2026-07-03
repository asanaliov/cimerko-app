// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.querySelectorAll("[data-listing-type-form]").forEach(form => {
    const typeSelect = form.querySelector("[data-listing-type]");
    const typeHelp = form.querySelector("[data-listing-type-help]");
    const roommatesField = form.querySelector("[data-roommates-needed-field]");
    const roommatesInput = form.querySelector("[data-roommates-needed]");
    const factsFields = form.querySelector("[data-listing-facts-fields]");

    if (!typeSelect || !typeHelp || !roommatesField || !roommatesInput) {
        return;
    }

    const lookingForPlaceValue = "1";
    const lookingForRoommateValue = "2";

    const updateListingTypeFields = () => {
        const isLookingForPlace = typeSelect.value === lookingForPlaceValue;
        const isLookingForRoommate = typeSelect.value === lookingForRoommateValue;

        typeHelp.textContent = isLookingForPlace
            ? "You need a room or apartment to live in."
            : isLookingForRoommate
                ? "You have or know a place and need one or more roommates."
                : "Choose whether you need a place or need roommates for a place.";

        roommatesField.hidden = !isLookingForRoommate;
        roommatesInput.disabled = !isLookingForRoommate;
        roommatesInput.required = isLookingForRoommate;
        factsFields?.classList.toggle("is-looking-for-place", isLookingForPlace);
    };

    typeSelect.addEventListener("change", updateListingTypeFields);
    updateListingTypeFields();
});

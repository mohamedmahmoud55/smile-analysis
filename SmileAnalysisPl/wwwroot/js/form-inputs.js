(function () {
    function initIntlPhoneInputs() {
        if (typeof window.intlTelInput !== "function") {
            return;
        }

        document.querySelectorAll(".cc-intl-phone").forEach(function (input) {
            if (input.dataset.intlTelInitialized === "true") {
                return;
            }

            var field = input.closest(".cc-intl-phone-field");
            var iti = window.intlTelInput(input, {
                initialCountry: "auto",
                nationalMode: false,
                separateDialCode: true,
                autoPlaceholder: "aggressive",
                formatOnDisplay: true,
                utilsScript: "https://cdn.jsdelivr.net/npm/intl-tel-input@24/build/js/utils.js",
                geoIpLookup: function (callback) {
                    fetch("https://ipapi.co/json/")
                        .then(function (response) { return response.json(); })
                        .then(function (data) { callback(data.country_code || "us"); })
                        .catch(function () { callback("us"); });
                }
            });

            input.dataset.intlTelInitialized = "true";
            input._itiInstance = iti;

            if (input.value) {
                iti.setNumber(input.value);
            }

            input.addEventListener("blur", function () {
                clearPhoneError(field);
            });
        });
    }

    function clearPhoneError(field) {
        if (!field) {
            return;
        }

        field.classList.remove("is-invalid");
        var error = field.querySelector(".cc-phone-error");
        if (error) {
            error.textContent = "";
        }
    }

    function showPhoneError(field, message) {
        if (!field) {
            return;
        }

        field.classList.add("is-invalid");
        var error = field.querySelector(".cc-phone-error");
        if (error) {
            error.textContent = message;
        }
    }

    function syncIntlPhoneValues(form) {
        var isValid = true;

        form.querySelectorAll(".cc-intl-phone").forEach(function (input) {
            var iti = input._itiInstance;
            if (!iti) {
                return;
            }

            var field = input.closest(".cc-intl-phone-field");
            clearPhoneError(field);

            var rawValue = input.value.trim();
            if (!rawValue) {
                input.value = "";
                return;
            }

            if (!iti.isValidNumber()) {
                showPhoneError(field, "Please enter a valid phone number for the selected country.");
                isValid = false;
                return;
            }

            input.value = iti.getNumber();
        });

        return isValid;
    }

    function initNumericOnlyInputs() {
        document.querySelectorAll(".cc-numeric-only").forEach(function (input) {
            if (input.dataset.numericOnlyInitialized === "true") {
                return;
            }

            input.dataset.numericOnlyInitialized = "true";
            input.setAttribute("inputmode", "numeric");
            input.setAttribute("pattern", "[0-9]*");

            input.addEventListener("input", function () {
                input.value = input.value.replace(/\D/g, "");
            });

            input.addEventListener("paste", function (event) {
                event.preventDefault();
                var pasted = (event.clipboardData || window.clipboardData).getData("text");
                input.value = pasted.replace(/\D/g, "");
            });
        });
    }

    function bindFormHandlers() {
        document.querySelectorAll("form").forEach(function (form) {
            if (form.dataset.formInputsBound === "true") {
                return;
            }

            if (!form.querySelector(".cc-intl-phone") && !form.querySelector(".cc-numeric-only")) {
                return;
            }

            form.dataset.formInputsBound = "true";
            form.addEventListener("submit", function (event) {
                if (form.querySelector(".cc-intl-phone") && !syncIntlPhoneValues(form)) {
                    event.preventDefault();
                    event.stopImmediatePropagation();
                }
            });
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        initIntlPhoneInputs();
        initNumericOnlyInputs();
        bindFormHandlers();
    });
})();

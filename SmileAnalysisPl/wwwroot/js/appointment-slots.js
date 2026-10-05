(function (window) {
    'use strict';

    function initAppointmentSlots(options) {
        var doctorEl = document.querySelector(options.doctorSelector);
        var dateEl = document.querySelector(options.dateSelector);
        var timeSel = document.querySelector(options.timeSelector);
        var gridEl = document.querySelector(options.gridSelector);
        var hintEl = document.querySelector(options.hintSelector);
        var urlBase = options.urlBase;
        var excludeAppointmentId = options.excludeAppointmentId || null;
        var initialTime = options.initialTime || '';

        if (!doctorEl || !dateEl || !timeSel || !gridEl || !hintEl)
            return;

        function setHint(text) {
            hintEl.textContent = text;
        }

        function clearGrid(message) {
            gridEl.innerHTML = '';
            gridEl.classList.add('d-none');
            timeSel.innerHTML = '';
            timeSel.add(new Option(message || '— Select doctor and date —', ''));
            timeSel.value = '';
        }

        function formatTime12(hm) {
            var parts = hm.split(':');
            var h = parseInt(parts[0], 10);
            var m = parts[1] || '00';
            if (isNaN(h)) return hm;
            var ampm = h >= 12 ? 'PM' : 'AM';
            var h12 = h % 12;
            if (h12 === 0) h12 = 12;
            return h12 + ':' + m + ' ' + ampm;
        }

        function renderSlots(slots, preserve) {
            timeSel.innerHTML = '';
            gridEl.innerHTML = '';
            gridEl.classList.remove('d-none');

            if (slots.length === 0) {
                clearGrid('No slots available');
                gridEl.classList.remove('d-none');
                gridEl.innerHTML = '<p class="cc-slot-empty mb-0">No available times on this day. Try another date or update the doctor\'s weekly availability.</p>';
                setHint('This doctor has no availability on that weekday, or every slot is already booked.');
                return;
            }

            timeSel.add(new Option('— Choose start time —', ''));
            slots.forEach(function (hm) {
                var label = formatTime12(hm.length >= 5 ? hm.substring(0, 5) : hm);
                timeSel.add(new Option(label, hm));

                var btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'cc-slot-btn';
                btn.textContent = label;
                btn.dataset.value = hm;
                btn.addEventListener('click', function () {
                    timeSel.value = hm;
                    gridEl.querySelectorAll('.cc-slot-btn').forEach(function (b) {
                        b.classList.toggle('cc-slot-btn-active', b.dataset.value === hm);
                    });
                });
                gridEl.appendChild(btn);
            });

            setHint(slots.length + ' time slot' + (slots.length === 1 ? '' : 's') + ' available — click one to select.');
            if (preserve && slots.indexOf(preserve) >= 0) {
                timeSel.value = preserve;
                gridEl.querySelectorAll('.cc-slot-btn').forEach(function (b) {
                    b.classList.toggle('cc-slot-btn-active', b.dataset.value === preserve);
                });
            }
        }

        async function loadSlots() {
            var doctorId = parseInt(doctorEl.value, 10);
            var date = dateEl.value;
            var preserve = timeSel.value || initialTime;

            if (!doctorId || !date) {
                clearGrid('— Select doctor and date —');
                setHint('Choose a doctor and a date to see available start times.');
                return;
            }

            setHint('Loading available times…');
            gridEl.innerHTML = '<div class="cc-slot-loading"><span class="spinner-border spinner-border-sm text-primary" role="status"></span> Checking schedule…</div>';
            gridEl.classList.remove('d-none');

            try {
                var u = new URL(urlBase, window.location.origin);
                u.searchParams.set('doctorId', doctorId);
                u.searchParams.set('date', date);
                if (excludeAppointmentId)
                    u.searchParams.set('excludeAppointmentId', excludeAppointmentId);

                var res = await fetch(u.toString());
                var data = await res.json();
                renderSlots(data.slots || [], preserve);
            } catch (e) {
                clearGrid('Could not load slots');
                gridEl.classList.remove('d-none');
                gridEl.innerHTML = '<p class="cc-slot-empty text-danger mb-0">Could not load available times. Check your connection and try again.</p>';
                setHint('Network error loading slots.');
            }
        }

        doctorEl.addEventListener('change', loadSlots);
        dateEl.addEventListener('change', loadSlots);
        loadSlots();
    }

    window.initAppointmentSlots = initAppointmentSlots;
})(window);

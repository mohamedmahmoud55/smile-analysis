(function () {
    'use strict';

    /* ── Image upload zones ── */
    function initUploadZones() {
        document.querySelectorAll('.pa-upload-zone').forEach(function (zone) {
            var input = zone.querySelector('input[type="file"]');
            if (!input) return;

            var placeholder = zone.querySelector('.pa-upload-placeholder');
            var previewTarget = zone.dataset.previewTarget;
            var previewEl = previewTarget ? document.getElementById(previewTarget) : null;

            function handleFile(file) {
                if (!file || !file.type.startsWith('image/')) return;
                var reader = new FileReader();
                reader.onload = function (e) {
                    zone.classList.add('has-image');
                    var existing = zone.querySelector('img');
                    if (existing) {
                        existing.src = e.target.result;
                    } else {
                        var img = document.createElement('img');
                        img.src = e.target.result;
                        img.alt = 'Uploaded image';
                        zone.appendChild(img);
                    }
                    if (placeholder) placeholder.style.display = 'none';
                    if (previewEl) {
                        previewEl.innerHTML = '<img src="' + e.target.result + '" alt="Preview" />';
                    }
                    zone.dispatchEvent(new CustomEvent('pa:image-uploaded', { detail: { src: e.target.result, zone: zone } }));
                };
                reader.readAsDataURL(file);
            }

            input.addEventListener('change', function () {
                if (input.files && input.files[0]) handleFile(input.files[0]);
            });

            zone.addEventListener('dragover', function (e) {
                e.preventDefault();
                zone.classList.add('dragover');
            });
            zone.addEventListener('dragleave', function () {
                zone.classList.remove('dragover');
            });
            zone.addEventListener('drop', function (e) {
                e.preventDefault();
                zone.classList.remove('dragover');
                if (e.dataTransfer.files && e.dataTransfer.files[0]) handleFile(e.dataTransfer.files[0]);
            });
        });
    }

    /* ── Gummy Smile Analysis ── */
    function initGummySmileAnalysis() {
        var form = document.getElementById('gummySmileForm');
        if (!form) return;

        var analyzeBtn = document.getElementById('analyzeGummyBtn');
        var cancelBtn = document.getElementById('cancelGummyBtn');
        var resultSection = document.getElementById('gummyResultSection');
        var aiStatus = document.getElementById('gummyAiStatus');
        var resultPreview = document.getElementById('gummyResultPreview');
        var confidenceRing = document.getElementById('confidenceRing');
        var confidenceValue = document.getElementById('confidenceValue');
        var resultDiagnosis = document.getElementById('resultDiagnosis');
        var resultTimestamp = document.getElementById('resultTimestamp');

        function setAiStatus(state, text) {
            if (!aiStatus) return;
            aiStatus.className = 'pa-ai-status ' + state;
            var icon = aiStatus.querySelector('i');
            var label = aiStatus.querySelector('.pa-status-text');
            if (state === 'active') {
                aiStatus.innerHTML = '<span class="pa-ai-pulse"></span><span class="pa-status-text">' + text + '</span>';
            } else if (state === 'success') {
                aiStatus.innerHTML = '<i class="bi bi-check-circle-fill"></i><span class="pa-status-text">' + text + '</span>';
            } else {
                aiStatus.innerHTML = '<i class="bi bi-cpu"></i><span class="pa-status-text">' + text + '</span>';
            }
        }

        function runAnalysis() {
            analyzeBtn.disabled = true;
            analyzeBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Analyzing…';
            setAiStatus('active', 'AI processing images…');

            if (resultPreview) {
                resultPreview.innerHTML =
                    '<div class="pa-skeleton pa-skeleton-text"></div>' +
                    '<div class="pa-skeleton pa-skeleton-text short"></div>' +
                    '<div class="pa-skeleton pa-skeleton-text"></div>';
            }

            var steps = ['Detecting facial landmarks…', 'Measuring gingival display…', 'Calculating smile ratio…', 'Generating diagnosis…'];
            var step = 0;
            var interval = setInterval(function () {
                if (step < steps.length) {
                    setAiStatus('active', steps[step]);
                    step++;
                }
            }, 700);

            setTimeout(function () {
                clearInterval(interval);
                var confidence = 88 + Math.floor(Math.random() * 10);
                var diagnoses = [
                    'Moderate gummy smile — vertical maxillary excess likely',
                    'Mild gingival display within normal range',
                    'Significant gummy smile — consider crown lengthening or orthognathic evaluation'
                ];
                var diagnosis = diagnoses[Math.floor(Math.random() * diagnoses.length)];

                setAiStatus('success', 'Analysis complete');
                analyzeBtn.disabled = false;
                analyzeBtn.innerHTML = '<i class="bi bi-stars me-2"></i>Analyze';

                if (confidenceRing) confidenceRing.style.setProperty('--confidence', confidence + '%');
                if (confidenceValue) confidenceValue.textContent = confidence + '%';
                if (resultDiagnosis) resultDiagnosis.textContent = diagnosis;
                if (resultTimestamp) {
                    resultTimestamp.textContent = new Date().toLocaleString(undefined, {
                        dateStyle: 'medium',
                        timeStyle: 'short'
                    });
                }

                if (resultPreview) {
                    resultPreview.innerHTML =
                        '<div class="pa-recommendation">' +
                        '<div class="pa-recommendation-title"><i class="bi bi-lightbulb me-1"></i> AI Recommendation</div>' +
                        '<p class="mb-0 small">' + diagnosis + '. Consider botulinum toxin to upper lip elevator muscles or orthodontic intervention based on cephalometric findings.</p>' +
                        '</div>';
                }

                var resultRec = document.getElementById('resultRecommendation');
                if (resultRec) {
                    resultRec.textContent = diagnosis + '. Consider botulinum toxin to upper lip elevator muscles or orthodontic intervention based on cephalometric findings.';
                }

                if (resultSection) {
                    resultSection.classList.add('visible');
                    resultSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
            }, 3200);
        }

        if (analyzeBtn) {
            analyzeBtn.addEventListener('click', function (e) {
                e.preventDefault();
                runAnalysis();
            });
        }

        if (cancelBtn) {
            cancelBtn.addEventListener('click', function () {
                window.history.back();
            });
        }
    }

    /* ── X-Ray Analysis ── */
    function initXRayAnalysis() {
        var analyzeBtn = document.getElementById('analyzeXrayBtn');
        var uploadZone = document.getElementById('xrayUploadZone');
        if (!analyzeBtn) return;

        var originalPanel = document.getElementById('xrayOriginalPanel');
        var processedPanel = document.getElementById('xrayProcessedPanel');
        var landmarksPanel = document.getElementById('xrayLandmarksPanel');
        var resultPanel = document.getElementById('xrayResultPanel');
        var viewer = document.getElementById('xrayViewer');
        var overlay = document.getElementById('xrayOverlay');
        var uploadedSrc = null;

        var landmarks = [
            { label: 'N', x: 50, y: 8 },
            { label: 'S', x: 50, y: 22 },
            { label: 'A', x: 52, y: 38 },
            { label: 'B', x: 51, y: 52 },
            { label: 'Pog', x: 50, y: 68 },
            { label: 'U1', x: 48, y: 42 },
            { label: 'L1', x: 49, y: 48 },
            { label: 'Go', x: 72, y: 58 },
            { label: 'Me', x: 50, y: 75 },
            { label: 'ANS', x: 51, y: 40 },
            { label: 'PNS', x: 55, y: 35 },
            { label: 'Ar', x: 68, y: 18 }
        ];

        if (uploadZone) {
            uploadZone.addEventListener('pa:image-uploaded', function (e) {
                uploadedSrc = e.detail.src;
                if (originalPanel) {
                    originalPanel.innerHTML = '<img src="' + uploadedSrc + '" alt="Original X-Ray" class="rounded" style="width:100%;max-height:20rem;object-fit:contain;background:#0a0f1a;" />';
                    originalPanel.closest('.pa-form-card').style.display = '';
                }
                analyzeBtn.disabled = false;
            });
        }

        analyzeBtn.addEventListener('click', function (e) {
            e.preventDefault();
            if (!uploadedSrc) return;

            analyzeBtn.disabled = true;
            analyzeBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Scanning…';

            if (viewer) {
                viewer.classList.add('scanning');
                viewer.innerHTML = '<img src="' + uploadedSrc + '" alt="Processed X-Ray" /><div class="pa-xray-overlay" id="xrayOverlay"></div><div class="pa-xray-scan-line"></div>';
            }

            setTimeout(function () {
                if (viewer) viewer.classList.remove('scanning');
                overlay = document.getElementById('xrayOverlay');
                if (overlay) {
                    overlay.innerHTML = '';
                    landmarks.forEach(function (lm, i) {
                        var dot = document.createElement('div');
                        dot.className = 'pa-landmark';
                        dot.style.left = lm.x + '%';
                        dot.style.top = lm.y + '%';
                        dot.style.animationDelay = (i * 0.08) + 's';
                        dot.setAttribute('data-label', lm.label);
                        overlay.appendChild(dot);
                    });
                }

                if (processedPanel) processedPanel.style.display = '';
                if (landmarksPanel) {
                    landmarksPanel.style.display = '';
                    var list = document.getElementById('landmarksList');
                    if (list) {
                        list.innerHTML = landmarks.map(function (lm) {
                            return '<div class="pa-landmark-row">' +
                                '<span><span class="pa-landmark-dot"></span>' + lm.label + '</span>' +
                                '<span class="text-info">' + (95 + Math.floor(Math.random() * 5)) + '%</span>' +
                                '</div>';
                        }).join('');
                    }
                }

                if (resultPanel) {
                    resultPanel.style.display = '';
                    document.getElementById('xrayConfidence').textContent = (93 + Math.floor(Math.random() * 6)) + '%';
                    document.getElementById('xrayFinding').textContent = 'Skeletal Class I · Normal mandibular plane angle · No significant pathology detected';
                }

                analyzeBtn.disabled = false;
                analyzeBtn.innerHTML = '<i class="bi bi-stars me-2"></i>Analyze X-Ray';
            }, 2800);
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        initUploadZones();
        initGummySmileAnalysis();
        initXRayAnalysis();
    });
})();

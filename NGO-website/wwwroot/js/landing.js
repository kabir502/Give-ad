/**
 * GIVE-AID Landing — Canvas frame sequence + GSAP ScrollTrigger
 * Frames: /images/frames/frame_001.png … frame_235.png
 */
(function () {
    'use strict';

    const FRAME_COUNT = 235;
    const FRAME_BASE = '/images/frames/';
    const FRAME_EXT = '.png';
    const FRAME_PAD = 3;
    const FRAME_W = 1280;
    const FRAME_H = 720;
    const UNLOCK_THRESHOLD = 0.4;

    function framePath(index) {
        const n = String(index + 1).padStart(FRAME_PAD, '0');
        return FRAME_BASE + 'frame_' + n + FRAME_EXT;
    }

    function prefersReducedMotion() {
        return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    }

    function createImageSequence(canvas) {
        const ctx = canvas.getContext('2d', { alpha: false });
        const images = new Array(FRAME_COUNT);
        let loaded = 0;
        let current = 0;
        let ready = false;
        let drawPending = false;
        let naturalW = FRAME_W;
        let naturalH = FRAME_H;

        function resize() {
            const dpr = Math.min(window.devicePixelRatio || 1, 2);
            const parent = canvas.parentElement;
            const rect = parent.getBoundingClientRect();
            const cssW = Math.max(1, Math.floor(rect.width));
            const cssH = Math.max(1, Math.floor(rect.height));
            canvas.width = Math.floor(cssW * dpr);
            canvas.height = Math.floor(cssH * dpr);
            canvas.style.width = cssW + 'px';
            canvas.style.height = cssH + 'px';
            ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
            render(current);
        }

        function drawCover(img, cssW, cssH) {
            const iw = img.naturalWidth || naturalW;
            const ih = img.naturalHeight || naturalH;
            const scale = Math.max(cssW / iw, cssH / ih);
            const dw = iw * scale;
            const dh = ih * scale;
            const dx = (cssW - dw) / 2;
            const dy = (cssH - dh) / 2;
            ctx.fillStyle = '#061525';
            ctx.fillRect(0, 0, cssW, cssH);
            ctx.drawImage(img, dx, dy, dw, dh);
        }

        function render(index) {
            const i = Math.max(0, Math.min(FRAME_COUNT - 1, Math.round(index)));
            current = i;
            const img = images[i];
            const cssW = canvas.clientWidth;
            const cssH = canvas.clientHeight;

            if (img && img.complete && img.naturalWidth) {
                drawCover(img, cssW, cssH);
                return;
            }

            for (let d = 0; d < FRAME_COUNT; d++) {
                const a = i - d;
                const b = i + d;
                if (a >= 0 && images[a] && images[a].naturalWidth) {
                    drawCover(images[a], cssW, cssH);
                    return;
                }
                if (b < FRAME_COUNT && images[b] && images[b].naturalWidth) {
                    drawCover(images[b], cssW, cssH);
                    return;
                }
            }
        }

        function scheduleRender(index) {
            current = index;
            if (drawPending) return;
            drawPending = true;
            requestAnimationFrame(function () {
                drawPending = false;
                render(current);
            });
        }

        function setProgress(p) {
            scheduleRender(Math.max(0, Math.min(1, p)) * (FRAME_COUNT - 1));
        }

        function loadAll(onProgress, onReady) {
            let notifiedReady = false;
            const priority = [];
            const step = 8;
            for (let i = 0; i < FRAME_COUNT; i += step) priority.push(i);
            for (let i = 0; i < FRAME_COUNT; i++) {
                if (priority.indexOf(i) === -1) priority.push(i);
            }

            let cursor = 0;
            let active = 0;
            const CONCURRENCY = 12;

            function check() {
                const pct = loaded / FRAME_COUNT;
                if (onProgress) onProgress(pct, loaded, FRAME_COUNT);
                if (!notifiedReady && (pct >= UNLOCK_THRESHOLD || loaded === FRAME_COUNT)) {
                    notifiedReady = true;
                    ready = true;
                    if (onReady) onReady();
                }
            }

            function pump() {
                while (cursor < priority.length && active < CONCURRENCY) {
                    loadOne(priority[cursor++]);
                }
            }

            function loadOne(i) {
                active++;
                const img = new Image();
                img.decoding = 'async';
                img.onload = function () {
                    images[i] = img;
                    if (i === 0) {
                        naturalW = img.naturalWidth || FRAME_W;
                        naturalH = img.naturalHeight || FRAME_H;
                        render(0);
                    }
                    loaded++;
                    active--;
                    check();
                    pump();
                };
                img.onerror = function () {
                    loaded++;
                    active--;
                    check();
                    pump();
                };
                img.src = framePath(i);
            }

            pump();
        }

        return {
            resize: resize,
            setProgress: setProgress,
            render: render,
            loadAll: loadAll,
            get ready() { return ready; }
        };
    }

    function bindLoader() {
        const el = document.getElementById('lpLoader');
        const bar = document.getElementById('lpLoaderBar');
        const pctEl = document.getElementById('lpLoaderPct');
        let hidden = false;

        return {
            update: function (progress) {
                const p = Math.round(Math.min(1, progress) * 100);
                if (bar) bar.style.width = p + '%';
                if (pctEl) pctEl.textContent = p + '%';
            },
            done: function () {
                if (hidden || !el) return;
                hidden = true;
                el.classList.add('is-done');
                el.setAttribute('aria-busy', 'false');
                setTimeout(function () {
                    el.setAttribute('hidden', '');
                }, 750);
            }
        };
    }

    function initNavbar() {
        // Landing uses the shared site navbar (same as About); no overlay nav behavior.
    }

    function initCounters() {
        if (typeof gsap === 'undefined' || typeof ScrollTrigger === 'undefined') return;

        document.querySelectorAll('[data-counter]').forEach(function (el) {
            const target = parseFloat(el.getAttribute('data-counter')) || 0;
            const prefix = el.getAttribute('data-prefix') || '';
            const suffix = el.getAttribute('data-suffix') || '';
            const obj = { val: 0 };

            ScrollTrigger.create({
                trigger: el,
                start: 'top 85%',
                once: true,
                onEnter: function () {
                    gsap.to(obj, {
                        val: target,
                        duration: prefersReducedMotion() ? 0 : 1.6,
                        ease: 'power2.out',
                        onUpdate: function () {
                            el.textContent = prefix + Math.round(obj.val).toLocaleString('en-PK') + suffix;
                        }
                    });
                }
            });
        });
    }

    function initReveals() {
        if (typeof gsap === 'undefined' || typeof ScrollTrigger === 'undefined') return;

        if (prefersReducedMotion()) {
            document.querySelectorAll('.lp-reveal').forEach(function (el) {
                el.style.opacity = '1';
                el.style.transform = 'none';
            });
            document.querySelectorAll('.lp-progress__fill').forEach(function (bar) {
                bar.style.width = (bar.getAttribute('data-progress') || '0') + '%';
            });
            return;
        }

        gsap.utils.toArray('.lp-reveal').forEach(function (el) {
            gsap.fromTo(el,
                { opacity: 0, y: 32 },
                {
                    opacity: 1,
                    y: 0,
                    duration: 0.85,
                    ease: 'power3.out',
                    scrollTrigger: {
                        trigger: el,
                        start: 'top 88%',
                        once: true
                    }
                }
            );
        });

        document.querySelectorAll('.lp-progress__fill').forEach(function (bar) {
            const pct = bar.getAttribute('data-progress') || '0';
            ScrollTrigger.create({
                trigger: bar,
                start: 'top 90%',
                once: true,
                onEnter: function () {
                    bar.style.width = pct + '%';
                }
            });
        });
    }

    function initParallax() {
        if (typeof gsap === 'undefined' || typeof ScrollTrigger === 'undefined') return;
        if (prefersReducedMotion()) return;

        const img = document.querySelector('.lp-story__media img');
        if (!img) return;

        gsap.to(img, {
            yPercent: -12,
            ease: 'none',
            scrollTrigger: {
                trigger: '.lp-story__media',
                start: 'top bottom',
                end: 'bottom top',
                scrub: true
            }
        });
    }

    function animateHeroText() {
        if (typeof gsap === 'undefined') return;

        const targets = ['.lp-hero__brand-mark', '.lp-hero__title', '.lp-hero__lead', '.lp-hero__trust', '.lp-hero__actions .lp-btn', '.lp-scroll-hint'];

        if (prefersReducedMotion()) {
            gsap.set(targets, { opacity: 1, y: 0 });
            return;
        }

        gsap.set(targets, { opacity: 0, y: 24 });
        const tl = gsap.timeline({ defaults: { ease: 'power3.out' } });
        tl.to('.lp-hero__brand-mark', { opacity: 1, y: 0, duration: 0.65 })
            .to('.lp-hero__title', { opacity: 1, y: 0, duration: 0.85 }, '-=0.35')
            .to('.lp-hero__lead', { opacity: 1, y: 0, duration: 0.7 }, '-=0.5')
            .to('.lp-hero__trust', { opacity: 1, y: 0, duration: 0.55 }, '-=0.4')
            .to('.lp-hero__actions .lp-btn', { opacity: 1, y: 0, duration: 0.5, stagger: 0.1 }, '-=0.3')
            .to('.lp-scroll-hint', { opacity: 1, duration: 0.5 }, '-=0.2');
    }

    function initScrollSequence(sequence) {
        if (typeof gsap === 'undefined' || typeof ScrollTrigger === 'undefined') return;
        if (prefersReducedMotion()) {
            sequence.setProgress(0.4);
            return;
        }

        const hero = document.getElementById('lpHero');
        if (!hero) return;

        gsap.to({}, {
            ease: 'none',
            scrollTrigger: {
                trigger: hero,
                start: 'top top',
                end: '+=280%',
                pin: true,
                scrub: 0.45,
                anticipatePin: 1,
                invalidateOnRefresh: true,
                onUpdate: function (self) {
                    sequence.setProgress(self.progress);
                }
            }
        });

        const content = document.querySelector('.lp-hero__content');
        if (content) {
            gsap.to(content, {
                opacity: 0.4,
                y: -36,
                ease: 'none',
                scrollTrigger: {
                    trigger: hero,
                    start: 'top top',
                    end: '+=160%',
                    scrub: true
                }
            });
        }
    }

    function start() {
        const canvas = document.getElementById('lpHeroCanvas');
        initNavbar();

        if (!canvas) return;

        if (typeof gsap !== 'undefined' && typeof ScrollTrigger !== 'undefined') {
            gsap.registerPlugin(ScrollTrigger);
        }

        const loader = bindLoader();
        const sequence = createImageSequence(canvas);
        const reduced = prefersReducedMotion();
        let unlocked = false;

        let resizeTimer = null;
        window.addEventListener('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                sequence.resize();
                if (typeof ScrollTrigger !== 'undefined') ScrollTrigger.refresh();
            }, 120);
        });

        sequence.resize();

        if (!reduced) {
            document.documentElement.style.overflow = 'hidden';
        }

        function unlock() {
            if (unlocked) return;
            unlocked = true;
            document.documentElement.style.overflow = '';
            loader.done();
            document.body.classList.add('lp-ready');
            animateHeroText();

            if (typeof gsap !== 'undefined' && typeof ScrollTrigger !== 'undefined') {
                initScrollSequence(sequence);
                initCounters();
                initReveals();
                initParallax();
                ScrollTrigger.refresh();
            } else {
                sequence.setProgress(reduced ? 0.4 : 0);
            }
        }

        sequence.loadAll(
            function (pct) {
                loader.update(pct);
                if (pct >= UNLOCK_THRESHOLD || reduced) unlock();
                if (pct >= 1 && typeof ScrollTrigger !== 'undefined') ScrollTrigger.refresh();
            },
            function () {
                unlock();
            }
        );
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', start);
    } else {
        start();
    }
})();

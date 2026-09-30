/**
 * GIVE-AID site theme — subtle GSAP reveals for non-landing pages.
 * Does not touch navbar behavior.
 */
(function () {
    'use strict';

    if (document.body.classList.contains('is-landing')) return;

    function prefersReducedMotion() {
        return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    }

    function boot() {
        if (typeof gsap === 'undefined' || typeof ScrollTrigger === 'undefined') return;
        if (prefersReducedMotion()) return;

        gsap.registerPlugin(ScrollTrigger);

        var main = document.querySelector('main');
        if (!main) return;

        // Page headers
        main.querySelectorAll('.container > .text-center.mb-5, .container > .text-center.mb-4').forEach(function (el) {
            gsap.fromTo(el,
                { opacity: 0, y: 20 },
                {
                    opacity: 1, y: 0, duration: 0.75, ease: 'power3.out',
                    scrollTrigger: { trigger: el, start: 'top 92%', once: true }
                }
            );
        });

        // Card grids — stagger children
        main.querySelectorAll('.row').forEach(function (row) {
            var cards = row.querySelectorAll(':scope > [class*="col"] > .card, :scope > [class*="col"] > .welfare-card, :scope > [class*="col"] > .stat-card, :scope > [class*="col"] > a > .card');
            if (cards.length < 2) return;
            gsap.fromTo(cards,
                { opacity: 0, y: 24 },
                {
                    opacity: 1, y: 0, duration: 0.65, stagger: 0.08, ease: 'power3.out',
                    scrollTrigger: { trigger: row, start: 'top 90%', once: true }
                }
            );
        });

        // Standalone cards / tables / accordion not in multi-card rows
        main.querySelectorAll('.card, .welfare-card, .stat-card, .table-responsive, .accordion').forEach(function (el) {
            var col = el.closest('[class*="col"]');
            var row = el.closest('.row');
            if (row && col && row.querySelectorAll(':scope > [class*="col"] > .card, :scope > [class*="col"] > .welfare-card, :scope > [class*="col"] > .stat-card').length >= 2) {
                return;
            }
            if (el.dataset.gaRevealed) return;
            el.dataset.gaRevealed = '1';
            gsap.fromTo(el,
                { opacity: 0, y: 22 },
                {
                    opacity: 1, y: 0, duration: 0.7, ease: 'power3.out',
                    scrollTrigger: { trigger: el, start: 'top 92%', once: true }
                }
            );
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', boot);
    } else {
        boot();
    }
})();

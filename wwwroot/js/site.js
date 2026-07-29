// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
    const sidebar = document.getElementById("sidebar");
    const toggle = document.getElementById("sidebarToggle");
    const overlay = document.getElementById("sidebarOverlay");

    function abrirSidebar() {
        sidebar?.classList.add("show");
        overlay?.classList.add("show");
    }

    function cerrarSidebar() {
        sidebar?.classList.remove("show");
        overlay?.classList.remove("show");
    }

    toggle?.addEventListener("click", function () {
        if (sidebar?.classList.contains("show")) {
            cerrarSidebar();
        } else {
            abrirSidebar();
        }
    });

    overlay?.addEventListener("click", cerrarSidebar);

    document
        .querySelectorAll(".sidebar-link")
        .forEach(function (enlace) {
            enlace.addEventListener("click", function () {
                if (window.innerWidth < 992) {
                    cerrarSidebar();
                }
            });
        });
});
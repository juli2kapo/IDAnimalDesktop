// Copia un texto al portapapeles desde Blazor Server (que no puede tocar
// `navigator.clipboard` directamente). `navigator.clipboard` no existe fuera
// de un contexto seguro (http sin TLS), así que hay un fallback con
// execCommand — mismo patrón que copiarIdAnimal en PublicAnimal.razor.
function copiarAlPortapapeles(texto) {
    if (navigator.clipboard && window.isSecureContext) {
        return navigator.clipboard.writeText(texto).then(
            function () { return true; },
            function () { return false; });
    }

    var ta = document.createElement('textarea');
    ta.value = texto;
    ta.setAttribute('readonly', '');
    ta.style.position = 'absolute';
    ta.style.left = '-9999px';
    document.body.appendChild(ta);
    ta.select();
    var ok = false;
    try { ok = document.execCommand('copy'); } catch (e) { ok = false; }
    document.body.removeChild(ta);
    return Promise.resolve(ok);
}

function preventNonNumeric(e) {
    if (['Backspace', 'Delete', 'Tab', 'Escape', 'Enter', 'ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(e.key)) {
        return;
    }
    if (e.key >= '0' && e.key <= '9') {
        return;
    }
    if ((e.key === '.' || e.key === ',') && 
        (!e.target.value.includes('.') && !e.target.value.includes(','))) {
        return;
    }
    e.preventDefault();
}
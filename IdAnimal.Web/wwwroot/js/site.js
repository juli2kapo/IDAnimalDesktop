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
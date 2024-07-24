
document.addEventListener('DOMContentLoaded', function () {
    var infoModal = document.getElementById('infoModal');

    infoModal.addEventListener('show.bs.modal', function (event) {
        var button = event.relatedTarget;
        var surname = button.getAttribute('data-employee-surname');
        var vacations = button.getAttribute('data-employee-vacations')

        console.log(vacations);
        vacations = JSON.parse(vacations);

        // update title
        var title = document.getElementById('infoModalLabel');
        title.innerHTML = '<p>Vacations of ' + surname + '</p>';

        // Update the modal content
        var modalBody = document.getElementById('modal-body-content');
        modalBody.innerHTML = "";

        vacations.forEach(function (vacation) {
            modalBody.innerHTML += '<p>DA ' + vacation + ' A ' + vacation + '</p>';
        });
    });
    //TODO: SISTEMA VISUALIZZAZIONE FERIE
    function formatDate(dateString) {
        var options = { year: 'numeric', month: 'short', day: 'numeric' };
        return new Date(dateString).toLocaleDateString(undefined, options);
    }
});

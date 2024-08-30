
document.addEventListener('DOMContentLoaded', function () {
    var infoModal = document.getElementById('infoModal');

    infoModal.addEventListener('show.bs.modal', function (event) {
        var button = event.relatedTarget; // Button that triggered the modal
        var employeeSurname = button.getAttribute('data-employee-surname');
        var employeeVacations = button.getAttribute('data-employee-vacations');

        var modalTitle = infoModal.querySelector('.modal-title');
        var modalBody = infoModal.querySelector('.modal-body');

        // Set the title
        modalTitle.textContent = 'Vacations for ' + employeeSurname;

        // Parse the vacations JSON data
        var vacations = JSON.parse(employeeVacations);

        // Check if vacations is empty
        if (vacations.length > 0) {
            // Clear the modal body content
            modalBody.innerHTML = '';

            // Display the vacations
            vacations.forEach(function (vacation) {
                var vacationItem = document.createElement('p');
                vacationItem.textContent = 'FROM ' + vacation.StartDate.split('T')[0] + ' TO ' + vacation.EndDate.split('T')[0];
                modalBody.appendChild(vacationItem);
            });
        } else {
            modalBody.textContent = 'No vacations available.';
        }
    });
});

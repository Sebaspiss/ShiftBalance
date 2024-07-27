function eraseDatepicker() {
    $('#fromDate').val('');
    $('#toDate').val('');
}
function showLoading() {
    document.getElementById('loading-overlay').classList.remove('d-none');
}

function hideLoading() {
    document.getElementById('loading-overlay').classList.add('d-none');
}

// Example: Hide loading when page is fully loaded (you might need to adjust this depending on your logic)
window.onload = function () {
    hideLoading();
}

function submitForm(actionUrl) {
    // Get selected employees
    var selectedEmployees = [];
    document.querySelectorAll('input[name="employees"]:checked').forEach(function (checkbox) {
        selectedEmployees.push(checkbox.dataset.employee);
    });

    var fromDate = document.getElementById('fromDate').value;
    var toDate = document.getElementById('toDate').value;

    var formData = new FormData();
    formData.append('employees', JSON.stringify(selectedEmployees));
    formData.append('fromDate', fromDate);
    formData.append('toDate', toDate);

    fetch(actionUrl, {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            // Handle the server response here
            console.log('Success:', data);
            hideLoading();
            // Optionally, redirect or update the page based on the response
        })
        .catch(error => {
            console.error('Error:', error);
            hideLoading();
        });
}



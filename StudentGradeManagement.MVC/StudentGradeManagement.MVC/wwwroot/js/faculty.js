const dashboard = document.getElementById("facultyDashboard");
const gradesTableBody = document.getElementById("gradesTableBody");
const dashboardMessage = document.getElementById("dashboardMessage");

const token = dashboard.dataset.token;

const apiUrl = "https://localhost:7146/api/faculty/grades";

const addGradeButton = document.getElementById("addGradeButton");
const gradeFormSection = document.getElementById("gradeFormSection");
const gradeForm = document.getElementById("gradeForm");
const cancelGradeButton = document.getElementById("cancelGradeButton");
const saveGradeButton = document.getElementById("saveGradeButton");

let currentGrades = [];



async function getErrorMessage(response) {
    const errorData = await response.json().catch(() => null);

    if (errorData?.errors) {
        return Object.values(errorData.errors).flat().join("\n");
    }

    return errorData?.message ||
        errorData?.title ||
        `Request failed. Status code: ${response.status}`;
}



async function loadGrades() {
    try {
        dashboardMessage.textContent = "Loading student grades...";
        dashboardMessage.className = "";

        const response = await fetch(apiUrl, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Accept": "application/json"
            }
        });

        if (!response.ok) {
            if (response.status === 401) {
                throw new Error("Your session has expired. Please log in again.");
            }

            if (response.status === 403) {
                throw new Error("You are not authorized to view these grades.");
            }

            throw new Error(await getErrorMessage(response));
        }

        const grades = await response.json();

        currentGrades = grades;

        gradesTableBody.innerHTML = "";

        if (grades.length === 0) {
            dashboardMessage.textContent = "No student grades found.";
            return;
        }

        grades.forEach(grade => {
            const row = document.createElement("tr");

            const values = [
                grade.studentNumber,
                grade.studentName,
                grade.subjectCode,
                grade.subjectName,
                grade.grade,
                grade.remarks,
                grade.semester,
                grade.schoolYear
            ];

            values.forEach(value => {
                const cell = document.createElement("td");
                cell.textContent = value ?? "";
                row.appendChild(cell);
            });

            const actionCell = document.createElement("td");

            const editButton = document.createElement("button");
            editButton.type = "button";
            editButton.textContent = "Edit";
            editButton.className = "edit-button";
            editButton.dataset.action = "edit";
            editButton.dataset.id = grade.gradeId;

            const deleteButton = document.createElement("button");
            deleteButton.type = "button";
            deleteButton.textContent = "Delete";
            deleteButton.className = "delete-button";
            deleteButton.dataset.action = "delete";
            deleteButton.dataset.id = grade.gradeId;

            actionCell.appendChild(editButton);
            actionCell.appendChild(deleteButton);

            row.appendChild(actionCell);
            gradesTableBody.appendChild(row);
        });

        dashboardMessage.textContent =
            `Showing ${grades.length} grade record(s).`;

    } catch (error) {
        dashboardMessage.textContent = error.message;
        dashboardMessage.className = "error";

        console.error("Faculty Dashboard Error:", error);
    }
}



addGradeButton.addEventListener("click", () => {
    gradeForm.reset();

    document.getElementById("gradeId").value = "";
    document.getElementById("gradeFormTitle").textContent = "Add New Grade";

    saveGradeButton.textContent = "Save Grade";
    gradeFormSection.hidden = false;
});



cancelGradeButton.addEventListener("click", () => {
    gradeForm.reset();
    gradeFormSection.hidden = true;
});



gradeForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    const gradeId = document.getElementById("gradeId").value;

    const gradeData = {
        studentId: Number(document.getElementById("studentId").value),
        subjectCode: document.getElementById("subjectCode").value.trim(),
        subjectName: document.getElementById("subjectName").value.trim(),
        grade: Number(document.getElementById("grade").value),
        semester: document.getElementById("semester").value.trim(),
        schoolYear: document.getElementById("schoolYear").value.trim()
    };

    if (!Number.isInteger(gradeData.studentId) ||
        gradeData.studentId <= 0) {
        alert("Please enter a valid Student ID.");
        return;
    }

    if (!Number.isInteger(gradeData.grade) ||
        gradeData.grade < 0 ||
        gradeData.grade > 100) {
        alert("Grade must be a whole number between 0 and 100.");
        return;
    }

    const isEditing = gradeId !== "";

    const requestUrl = isEditing
        ? `${apiUrl}/${gradeId}`
        : apiUrl;

    const requestMethod = isEditing ? "PUT" : "POST";

    try {
        saveGradeButton.disabled = true;
        saveGradeButton.textContent = isEditing ? "Updating..." : "Saving...";

        const response = await fetch(requestUrl, {
            method: requestMethod,
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json",
                "Accept": "application/json"
            },
            body: JSON.stringify(gradeData)
        });

        if (!response.ok) {
            throw new Error(await getErrorMessage(response));
        }

        alert(
            isEditing
                ? "Grade successfully updated!"
                : "Grade successfully added!"
        );

        gradeForm.reset();
        gradeFormSection.hidden = true;

        await loadGrades();

    } catch (error) {
        alert(error.message);
        console.error("Save Grade Error:", error);

    } finally {
        saveGradeButton.disabled = false;
        saveGradeButton.textContent = "Save Grade";
    }
});



gradesTableBody.addEventListener("click", (event) => {
    const button = event.target.closest("button[data-action]");

    if (!button) {
        return;
    }

    const gradeId = Number(button.dataset.id);
    const action = button.dataset.action;

    if (action === "edit") {
        editGrade(gradeId);
    }

    if (action === "delete") {
        deleteGrade(gradeId);
    }
});


function editGrade(gradeId) {
    const grade = currentGrades.find(
        item => item.gradeId === gradeId
    );

    if (!grade) {
        alert("Grade record could not be found.");
        return;
    }

    document.getElementById("gradeFormTitle").textContent = "Edit Grade";
    document.getElementById("gradeId").value = grade.gradeId;

    document.getElementById("studentId").value = grade.studentId;
    document.getElementById("subjectCode").value = grade.subjectCode;
    document.getElementById("subjectName").value = grade.subjectName;
    document.getElementById("grade").value = grade.grade;
    document.getElementById("semester").value = grade.semester;
    document.getElementById("schoolYear").value = grade.schoolYear;

    saveGradeButton.textContent = "Update Grade";

    gradeFormSection.hidden = false;

    gradeFormSection.scrollIntoView({
        behavior: "smooth",
        block: "start"
    });
}



async function deleteGrade(gradeId) {
    const confirmed = confirm(
        "Are you sure you want to delete this grade record?"
    );

    if (!confirmed) {
        return;
    }

    try {
        const response = await fetch(`${apiUrl}/${gradeId}`, {
            method: "DELETE",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Accept": "application/json"
            }
        });

        if (!response.ok) {
            throw new Error(await getErrorMessage(response));
        }

        alert("Grade successfully deleted!");

        await loadGrades();

    } catch (error) {
        alert(error.message);
        console.error("Delete Grade Error:", error);
    }
}

loadGrades();

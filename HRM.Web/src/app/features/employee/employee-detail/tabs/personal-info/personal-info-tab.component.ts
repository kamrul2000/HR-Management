import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

import { EmployeeResponse } from '../../../models/employee.model';

@Component({
  selector: 'hrm-personal-info-tab',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="info-grid">
      <section>
        <h4 class="text-label">Identity</h4>
        <dl>
          <dt>Employee Code</dt><dd class="text-mono">{{ employee.employeeCode }}</dd>
          <dt>Full Name</dt><dd>{{ employee.fullName }}</dd>
          <dt>Email</dt><dd>{{ employee.email }}</dd>
          <dt>Phone</dt><dd>{{ employee.phone }}</dd>
          <dt>Date of Birth</dt><dd>{{ employee.dateOfBirthFormatted || employee.dateOfBirth }}</dd>
          <dt>Gender</dt><dd>{{ employee.gender }}</dd>
          <dt>Marital Status</dt><dd>{{ employee.maritalStatus }}</dd>
          <dt>National ID</dt><dd>{{ employee.nationalId || '—' }}</dd>
        </dl>
      </section>

      <section>
        <h4 class="text-label">Employment</h4>
        <dl>
          <dt>Branch</dt><dd>{{ employee.branchName || '—' }}</dd>
          <dt>Department</dt><dd>{{ employee.departmentName || '—' }}</dd>
          <dt>Designation</dt><dd>{{ employee.designationTitle || '—' }}</dd>
          <dt>Employment Type</dt><dd>{{ employee.employmentType }}</dd>
          <dt>Joining Date</dt><dd>{{ employee.joiningDateFormatted || employee.joiningDate }}</dd>
          <dt>Confirmation Date</dt><dd>{{ employee.confirmationDateFormatted || employee.confirmationDate || '—' }}</dd>
          <dt>Status</dt><dd>{{ employee.statusLabel || employee.status }}</dd>
        </dl>
      </section>

      <section class="info-grid__full">
        <h4 class="text-label">Address</h4>
        <p class="info-grid__address">{{ employee.address }}</p>
      </section>
    </div>
  `,
  styleUrl: './personal-info-tab.component.scss',
})
export class PersonalInfoTabComponent {
  @Input({ required: true }) employee!: EmployeeResponse;
}

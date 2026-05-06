export interface NavItem {
  label: string;
  path?: string;
  icon?: string;
  /** Optional permission module code from Module 34 */
  module?: string;
  children?: NavItem[];
}

import {
  heroSquares2x2,
  heroBuildingOffice,
  heroUserGroup,
  heroClock,
  heroCalendarDays,
  heroBriefcase,
  heroBanknotes,
  heroCreditCard,
  heroReceiptPercent,
  heroGiftTop,
  heroArrowLeftOnRectangle,
  heroLockClosed,
} from '@ng-icons/heroicons/outline';

export const NAV_ICONS = {
  heroSquares2x2,
  heroBuildingOffice,
  heroUserGroup,
  heroClock,
  heroCalendarDays,
  heroBriefcase,
  heroBanknotes,
  heroCreditCard,
  heroReceiptPercent,
  heroGiftTop,
  heroArrowLeftOnRectangle,
  heroLockClosed,
};

export const NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard', path: '/dashboard', icon: 'heroSquares2x2' },
  {
    label: 'Organization',
    icon: 'heroBuildingOffice',
    children: [
      { label: 'Company',     path: '/organization/companies',    module: 'COMPANY' },
      { label: 'Branches',    path: '/organization/branches',     module: 'BRANCH' },
      { label: 'Departments', path: '/organization/departments',  module: 'DEPARTMENT' },
      { label: 'Designations',path: '/organization/designations', module: 'DESIGNATION' },
    ],
  },
  {
    label: 'Employees',
    icon: 'heroUserGroup',
    children: [
      { label: 'All Employees',   path: '/employees',                  module: 'EMPLOYEE' },
      { label: 'Additional Info', path: '/employees/additional-info',  module: 'EMPLOYEE' },
    ],
  },
  {
    label: 'Attendance',
    icon: 'heroClock',
    children: [
      { label: 'Duty Slots',       path: '/attendance/duty-slots',       module: 'ATTENDANCE' },
      { label: 'Attendance',       path: '/attendance/records',          module: 'ATTENDANCE' },
      { label: 'Off Days',         path: '/attendance/off-days',         module: 'ATTENDANCE' },
      { label: 'Holiday Calendar', path: '/attendance/holiday-calendar', module: 'ATTENDANCE' },
    ],
  },
  {
    label: 'Leave Management',
    icon: 'heroCalendarDays',
    children: [
      { label: 'Leave Types',      path: '/leave/types',        module: 'LEAVE' },
      { label: 'Leave Allotments', path: '/leave/allotments',   module: 'LEAVE' },
      { label: 'Applications',     path: '/leave/applications', module: 'LEAVE' },
    ],
  },
  { label: 'Overtime', path: '/overtime', icon: 'heroClock', module: 'OVERTIME' },
  {
    label: 'Payroll',
    icon: 'heroBanknotes',
    children: [
      { label: 'Salary Heads',      path: '/salary/heads',        module: 'SALARY' },
      { label: 'Salary Structure',  path: '/salary/structures',   module: 'SALARY' },
      { label: 'Salary Processing', path: '/salary/calculations', module: 'SALARY' },
      { label: 'Bonus',             path: '/salary/bonus',        module: 'BONUS' },
    ],
  },
  {
    label: 'Loans',
    icon: 'heroCreditCard',
    children: [
      { label: 'Applications',  path: '/loans/applications',  module: 'LOAN' },
      { label: 'Approvals',     path: '/loans/approvals',     module: 'LOAN' },
      { label: 'Active Loans',  path: '/loans/active',        module: 'LOAN' },
      { label: 'Installments',  path: '/loans/installments',  module: 'LOAN' },
    ],
  },
  {
    label: 'Tax & PF',
    icon: 'heroReceiptPercent',
    children: [
      { label: 'Tax Slabs',         path: '/tax/slabs',          module: 'TAX' },
      { label: 'Tax Exclusions',    path: '/tax/exclusions',     module: 'TAX' },
      { label: 'PF Contributions',  path: '/pf/contributions',   module: 'PF' },
      { label: 'PF Interest',       path: '/pf/interest',        module: 'PF' },
    ],
  },
  {
    label: 'Gratuity',
    icon: 'heroGiftTop',
    children: [
      { label: 'Setup',     path: '/gratuity/rules',        module: 'GRATUITY' },
      { label: 'Calculate', path: '/gratuity/calculations', module: 'GRATUITY' },
    ],
  },
  { label: 'Separation', path: '/separation', icon: 'heroArrowLeftOnRectangle', module: 'SEPARATION' },
  {
    label: 'Access Control',
    icon: 'heroLockClosed',
    children: [
      { label: 'Roles',       path: '/access/roles',       module: 'ROLE' },
      { label: 'User Roles',  path: '/access/user-roles',  module: 'ROLE' },
      { label: 'Permissions', path: '/access/permissions', module: 'ROLE' },
    ],
  },
];

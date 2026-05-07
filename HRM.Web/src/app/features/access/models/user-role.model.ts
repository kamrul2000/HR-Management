export interface UserRoleResponse {
  id: number;
  userId: number;
  userName: string;
  userEmail: string;
  roleId: number;
  roleName: string;
  assignedById: number;
  assignedAt: string;
  assignedAtFormatted?: string;
  isActive: boolean;
  revokedAt?: string | null;
  revokedAtFormatted?: string | null;
  revokedById?: number | null;
  subscriptionId: number;
  createdAt: string;
}

export interface AssignRoleDto {
  userId: number;
  roleId: number;
}

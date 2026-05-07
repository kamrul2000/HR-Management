export interface RoleResponse {
  id: number;
  roleName: string;
  description?: string | null;
  isActive: boolean;
  userCount: number;
  permissionCount: number;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateRoleDto {
  roleName: string;
  description?: string | null;
}

export interface UpdateRoleDto {
  roleName: string;
  description?: string | null;
  isActive: boolean;
}

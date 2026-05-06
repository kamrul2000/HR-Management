export interface CompanyResponse {
  id: number;
  name: string;
  address: string;
  phone: string;
  email: string;
  website?: string | null;
  logoPath?: string | null;
  logoUrl?: string | null;
  isActive: boolean;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCompanyDto {
  name: string;
  address: string;
  phone: string;
  email: string;
  website?: string | null;
}

export interface UpdateCompanyDto {
  name: string;
  address: string;
  phone: string;
  email: string;
  website?: string | null;
  isActive: boolean;
}

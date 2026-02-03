export interface Feature {
  key: string;
  defaultState: boolean;
  description: string | null;
}

export interface CreateFeatureRequest {
  key: string;
  defaultState: boolean;
  description?: string | null;
}

export interface UpdateFeatureRequest {
  defaultState: boolean;
  description?: string | null;
}

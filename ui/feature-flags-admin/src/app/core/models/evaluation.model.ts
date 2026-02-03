export interface EvaluateFeatureRequest {
  userId?: string | null;
  groupIds?: string[] | null;
  region?: string | null;
}

export interface EvaluateFeatureResponse {
  enabled: boolean;
  source: string; // "Default" | "UserOverride" | "GroupOverride" | "RegionOverride"
}

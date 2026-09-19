export type EducationLevel =
  | 'LessThanSecondary'
  | 'Secondary'
  | 'OneYearPostSecondary'
  | 'TwoYearPostSecondary'
  | 'BachelorsOrThreeYear'
  | 'TwoOrMoreCredentials'
  | 'MastersOrProfessional'
  | 'Doctoral';

export type CanadianEducationLevel = 'None' | 'OneOrTwoYears' | 'ThreeYearsOrMore';

export interface LanguageAbilities {
  listening: number;
  reading: number;
  writing: number;
  speaking: number;
}

export interface SpouseInfo {
  education?: EducationLevel;
  firstLanguage?: LanguageAbilities;
  canadianWorkYears?: number;
}

export interface CrsScoreRequest {
  age: number;
  education: EducationLevel;
  firstLanguageIsFrench: boolean;
  firstLanguage: LanguageAbilities;
  secondLanguage?: LanguageAbilities;
  canadianWorkYears: number;
  foreignWorkYears: number;
  hasCertificateOfQualification: boolean;
  hasProvincialNomination: boolean;
  hasSiblingInCanada: boolean;
  canadianEducation: CanadianEducationLevel;
  spouse?: SpouseInfo;
}

export interface CrsBreakdown {
  age: number;
  education: number;
  firstLanguage: number;
  secondLanguage: number;
  canadianWorkExperience: number;
  spouseFactors: number;
  skillTransferability: number;
  additional: number;
}

export interface CrsScoreResult {
  totalScore: number;
  breakdown: CrsBreakdown;
}

export type CategoryEligibility = 'Eligible' | 'NotEligible' | 'Unknown';

export interface CategoryInvitation {
  category: string;
  latestDrawNumber: number;
  latestDrawDate: string;
  latestCutoff: number;
  wouldBeInvited: boolean;
  eligibility: CategoryEligibility;
}

export interface PoolPosition {
  range: string;
  candidatesInRange: number;
  candidatesBelow: number;
  totalCandidates: number;
  percentBelow: number;
}

export interface InvitationAnalysis {
  score: number;
  categories: CategoryInvitation[];
  pool?: PoolPosition;
}

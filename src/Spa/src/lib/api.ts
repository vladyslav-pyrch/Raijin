import {CombinatoricsApiService} from '../services/combinatorics';

export const api = new CombinatoricsApiService(
  import.meta.env['VITE_COMBINATORICS_API_URL'] ?? '',
);

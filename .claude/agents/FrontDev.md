---
name: frontend-developer
description: Expert React 19 + TypeScript developer specializing in modern SPA development
disallowedTools: []
tags:
  - frontend
  - react
  - typescript
  - spa
  - vite
visibility: workspace
handoffs:
  - label: Plan Feature
    agent: Plan
    prompt: Create a step-by-step implementation plan for this frontend feature with component structure and state management.
  - label: Review Code
    agent: Review
    prompt: Review the implemented frontend code for quality, React best practices, and TypeScript type safety.
---

# Frontend Developer Agent

## Role

Expert React 19 + TypeScript developer specializing in modern SPA development for the Raijin project.

## Activation

Trigger this agent when working on:

- React components (functional components, hooks)
- TypeScript types and interfaces
- State management (Zustand, TanStack Query)
- API integration with backend
- Vite configuration
- UI/UX implementation

## Responsibilities

1. **Build modern React components** using hooks and functional patterns
2. **Write type-safe TypeScript** with strict type checking
3. **Integrate with backend APIs** through the API Gateway
4. **Manage state effectively** using appropriate libraries
5. **Follow React best practices** and performance patterns
6. **Ensure accessibility** (ARIA attributes, semantic HTML)

## React Patterns

### Functional Components

**Always use functional components** with hooks (not class components):

```tsx
import { useState, useEffect } from "react"

interface ProblemListProps {
	userId: string
}

export function ProblemList({ userId }: ProblemListProps) {
	const [problems, setProblems] = useState<Problem[]>([])
	const [loading, setLoading] = useState(true)

	useEffect(() => {
		fetchProblems()
	}, [userId])

	const fetchProblems = async () => {
		const data = await problemsApi.getAll()
		setProblems(data)
		setLoading(false)
	}

	if (loading) return <div>Loading...</div>

	return (
		<ul>
			{problems.map((p) => (
				<li key={p.id}>{p.name}</li>
			))}
		</ul>
	)
}
```

### Custom Hooks

Extract reusable logic into custom hooks:

```tsx
// hooks/useProblems.ts
import { useState, useEffect } from "react"
import { problemsApi } from "../services/api/problemsApi"

export function useProblems() {
	const [problems, setProblems] = useState<Problem[]>([])
	const [loading, setLoading] = useState(true)
	const [error, setError] = useState<string | null>(null)

	useEffect(() => {
		loadProblems()
	}, [])

	const loadProblems = async () => {
		try {
			const data = await problemsApi.getAll()
			setProblems(data)
		} catch (err) {
			setError(err instanceof Error ? err.message : "Failed to load")
		} finally {
			setLoading(false)
		}
	}

	const create = async (name: string, description: string) => {
		const newProblem = await problemsApi.create(name, description)
		setProblems((prev) => [...prev, newProblem])
	}

	return { problems, loading, error, create }
}

// Usage in component
export function ProblemsPage() {
	const { problems, loading, error, create } = useProblems()

	if (loading) return <Loading />
	if (error) return <Error message={error} />

	return <ProblemList problems={problems} onCreate={create} />
}
```

## TypeScript Guidelines

### Strict Type Safety

**Enable strict mode** in `tsconfig.json`:

```json
{
	"compilerOptions": {
		"strict": true,
		"noUncheckedIndexedAccess": true,
		"noImplicitReturns": true,
		"noFallthroughCasesInSwitch": true
	}
}
```

### Type Definitions

```typescript
// types/problem.ts
export interface Problem {
	id: string
	name: string
	description: string
	status: ProblemStatus
	createdAt: string
}

export enum ProblemStatus {
	Pending = "Pending",
	Running = "Running",
	Completed = "Completed",
	Failed = "Failed",
}

// API Response types
export interface CreateProblemRequest {
	name: string
	description: string
}

export interface CreateProblemResponse {
	problemId: string
}

export interface ApiError {
	title: string
	status: number
	errors?: Record<string, string[]>
}
```

### Type-Safe API Client

```typescript
// services/api/problemsApi.ts
import { apiClient } from "./client"
import type {
	Problem,
	CreateProblemRequest,
	CreateProblemResponse,
} from "../../types"

export const problemsApi = {
	async getAll(): Promise<Problem[]> {
		const response = await apiClient.get<Problem[]>("/v1/problems")
		return response.data
	},

	async getById(id: string): Promise<Problem> {
		const response = await apiClient.get<Problem>(`/v1/problems/${id}`)
		return response.data
	},

	async create(
		name: string,
		description: string,
	): Promise<CreateProblemResponse> {
		const request: CreateProblemRequest = { name, description }
		const response = await apiClient.post<CreateProblemResponse>(
			"/v1/problems",
			request,
		)
		return response.data
	},

	async delete(id: string): Promise<void> {
		await apiClient.delete(`/v1/problems/${id}`)
	},
}
```

## State Management

### Using Zustand (Recommended for Simple State)

```typescript
// stores/problemsStore.ts
import { create } from "zustand"
import { problemsApi } from "../services/api/problemsApi"

interface ProblemsState {
	problems: Problem[]
	loading: boolean
	error: string | null
	fetchProblems: () => Promise<void>
	createProblem: (name: string, description: string) => Promise<void>
}

export const useProblemsStore = create<ProblemsState>((set) => ({
	problems: [],
	loading: false,
	error: null,

	fetchProblems: async () => {
		set({ loading: true, error: null })
		try {
			const problems = await problemsApi.getAll()
			set({ problems, loading: false })
		} catch (error) {
			set({ error: "Failed to fetch problems", loading: false })
		}
	},

	createProblem: async (name, description) => {
		try {
			const response = await problemsApi.create(name, description)
			const newProblem = await problemsApi.getById(response.problemId)
			set((state) => ({ problems: [...state.problems, newProblem] }))
		} catch (error) {
			set({ error: "Failed to create problem" })
		}
	},
}))
```

### Using TanStack Query (For Server State)

```typescript
// hooks/useProblems.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { problemsApi } from '../services/api/problemsApi';

export function useProblems() {
  return useQuery({
    queryKey: ['problems'],
    queryFn: () => problemsApi.getAll()
  });
}

export function useCreateProblem() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ name, description }: { name: string; description: string }) =>
      problemsApi.create(name, description),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['problems'] });
    }
  });
}

// Usage
export function ProblemsPage() {
  const { data: problems, isLoading, error } = useProblems();
  const createMutation = useCreateProblem();

  const handleCreate = async (name: string, desc: string) => {
    await createMutation.mutateAsync({ name, description: desc });
  };

  if (isLoading) return <Loading />;
  if (error) return <Error message={error.message} />;

  return <ProblemList problems={problems} onCreate={handleCreate} />;
}
```

## Component Patterns

### Props Interface

```typescript
interface ButtonProps {
  variant?: 'primary' | 'secondary' | 'danger';
  onClick?: () => void;
  disabled?: boolean;
  children: React.ReactNode;
}

export function Button({ variant = 'primary', onClick, disabled, children }: ButtonProps) {
  return (
    <button
      className={`btn btn-${variant}`}
      onClick={onClick}
      disabled={disabled}
    >
      {children}
    </button>
  );
}
```

### Conditional Rendering

```tsx
// Early return for loading/error
if (loading) return <Loading />
if (error) return <Error message={error} />
if (!data) return null

// Ternary for simple conditions
{
	isActive ? <ActiveView /> : <InactiveView />
}

// Logical AND for optional rendering
{
	showDetails && <Details data={data} />
}
```

### Lists and Keys

```tsx
{
	problems.map((problem) => (
		<ProblemCard key={problem.id} problem={problem} />
	))
}
```

## API Integration

### Axios Client Setup

```typescript
// services/api/client.ts
import axios from "axios"

export const apiClient = axios.create({
	baseURL: import.meta.env.VITE_API_URL || "https://localhost:7108",
	timeout: 10000,
	headers: {
		"Content-Type": "application/json",
	},
})

// Request interceptor (add auth token)
apiClient.interceptors.request.use((config) => {
	const token = localStorage.getItem("auth_token")
	if (token) {
		config.headers.Authorization = `Bearer ${token}`
	}
	return config
})

// Response interceptor (handle errors)
apiClient.interceptors.response.use(
	(response) => response,
	(error) => {
		if (error.response?.status === 401) {
			// Handle unauthorized
			window.location.href = "/login"
		}
		return Promise.reject(error)
	},
)
```

## File Organization

### Component Structure

```typescript
// components/features/problems/ProblemCard/ProblemCard.tsx
import type { Problem } from '../../../types/problem';
import styles from './ProblemCard.module.css';

interface ProblemCardProps {
  problem: Problem;
  onDelete?: (id: string) => void;
}

export function ProblemCard({ problem, onDelete }: ProblemCardProps) {
  return (
    <div className={styles.card}>
      <h3 className={styles.title}>{problem.name}</h3>
      <p className={styles.description}>{problem.description}</p>
      {onDelete && (
        <button onClick={() => onDelete(problem.id)}>Delete</button>
      )}
    </div>
  );
}
```

### Barrel Exports

```typescript
// components/features/problems/index.ts
export { ProblemList } from "./ProblemList"
export { ProblemCard } from "./ProblemCard"
export { CreateProblemForm } from "./CreateProblemForm"

// Import elsewhere
import { ProblemList, ProblemCard } from "../components/features/problems"
```

## Best Practices

### ✅ DO

- Use functional components with hooks
- Define prop interfaces with TypeScript
- Extract logic into custom hooks
- Use CSS modules for scoped styling
- Memoize expensive computations (`useMemo`)
- Memoize callbacks passed to children (`useCallback`)
- Handle loading and error states
- Use proper React keys in lists
- Follow accessibility guidelines (ARIA, semantic HTML)

### ❌ DON'T

- Use class components
- Inline complex logic in JSX
- Mutate state directly (use `setState`)
- Forget dependency arrays in `useEffect`
- Use `any` type (use proper types)
- Fetch in render (use `useEffect` or TanStack Query)
- Ignore error handling

## Testing Frontend

```typescript
// ProblemList.test.tsx
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ProblemList } from './ProblemList';

describe('ProblemList', () => {
  it('renders problems correctly', () => {
    const problems = [
      { id: '1', name: 'Test Problem', description: 'Test Desc' }
    ];

    render(<ProblemList problems={problems} />);

    expect(screen.getByText('Test Problem')).toBeInTheDocument();
  });

  it('calls onCreate when button clicked', async () => {
    const onCreate = vi.fn();
    render(<ProblemList problems={[]} onCreate={onCreate} />);

    const button = screen.getByRole('button', { name: /create/i });
    await button.click();

    expect(onCreate).toHaveBeenCalledOnce();
  });
});
```

## References

See `.github/raijin-frontend-instructions.md` for comprehensive frontend development guide.
